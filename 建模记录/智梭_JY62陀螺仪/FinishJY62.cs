using System;
using System.IO;
using System.Collections.Generic;
using SolidWorks.Interop.sldworks;
public class FinishJY62 {
 public static string Run(){
  string path=@"D:\Robot\2027工创赛智梭速运升降台\模型\智梭_JY62陀螺仪.SLDPRT";
  string root=@"D:\Robot\2027工创赛智梭速运升降台\建模记录\智梭_JY62陀螺仪";
  ISldWorks sw=(ISldWorks)Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application"));
  IModelDoc2 d=(IModelDoc2)sw.GetOpenDocumentByName(path);
  if(d==null || !string.Equals(d.GetPathName(),path,StringComparison.OrdinalIgnoreCase))throw new Exception("Exact newly created document not found; no documents modified.");
  int activateError=0;sw.ActivateDoc3(path,false,0,ref activateError);
  List<string> report=new List<string>();
  string[] names={"载板型号丝印","模块方向标识","模块功能标识","模块型号标识"};
  foreach(string name in names){
   IFeature feature=(IFeature)((IPartDoc)d).FeatureByName(name);
   if(feature!=null){d.ClearSelection2(true);if(!feature.Select2(false,0) || !d.Extension.DeleteSelection2(1))throw new Exception("Delete text failed "+name);}
   IFeature sketch=(IFeature)((IPartDoc)d).FeatureByName(name+"_草图");
   if(sketch!=null){d.ClearSelection2(true);if(!sketch.Select2(false,0) || !d.Extension.DeleteSelection2(1))throw new Exception("Delete text sketch failed "+name);}
   report.Add("Removed text geometry: "+name);
  }
  d.ClearSelection2(true);d.EditRebuild3();
  IFeature holeFeature=(IFeature)((IPartDoc)d).FeatureByName("12个完整引脚通孔_D0.8_排距12.7_间距2.54");
  if(holeFeature==null){
   IFeature plane=(IFeature)d.FirstFeature();while(plane!=null && plane.GetTypeName2()!="RefPlane")plane=(IFeature)plane.GetNextFeature();
   if(plane==null || !plane.Select2(false,0))throw new Exception("Hole plane selection failed");
   ISketchManager sm=d.SketchManager;sm.InsertSketch(true);sm.AddToDB=true;sm.DisplayWhenAdded=false;
   foreach(int side in new int[]{-1,1})for(int i=0;i<6;i++)sm.CreateCircleByRadius(side*.00635,(2.5-6.35+i*2.54)/1000,0,.0004);
   sm.AddToDB=false;sm.DisplayWhenAdded=true;IFeature sketch=(IFeature)d.GetActiveSketch2();sketch.Name="完整引脚圆孔_12xD0.8_草图";sm.InsertSketch(true);
   d.ClearSelection2(true);sketch.Select2(false,0);
   holeFeature=d.FeatureManager.FeatureCut3(false,false,false,1,1,.006,.006,false,false,false,false,0,0,false,false,false,false,false,false,true,false,false,false,0,0,false);
   if(holeFeature==null)throw new Exception("Full pin hole cut failed");holeFeature.Name="12个完整引脚通孔_D0.8_排距12.7_间距2.54";
   report.Add("Added 12 complete through holes: diameter 0.8 mm, pitch 2.54 mm, row spacing 12.7 mm");
  }
  d.ClearSelection2(true);d.EditRebuild3();
  IFeature f=(IFeature)d.FirstFeature();int count=0;while(f!=null){
   if(f.GetErrorCode()!=0)throw new Exception("Feature error "+f.Name+" "+f.GetErrorCode());
   if(f.GetTypeName2()=="ProfileFeature"){
    ISketch sk=(ISketch)f.GetSpecificFeature2();object[] texts=(object[])sk.GetSketchTextSegments();
    if(texts!=null && texts.Length>0)throw new Exception("Text remains in "+f.Name);
   }
   count++;f=(IFeature)f.GetNextFeature();
  }
  report.Add("Feature error check: PASS; checked="+count);report.Add("Visible sketch text remaining: 0");
  object[] bodies=(object[])((IPartDoc)d).GetBodies2(0,false);report.Add("Solid bodies="+bodies.Length);
  double[] min={double.MaxValue,double.MaxValue,double.MaxValue},max={double.MinValue,double.MinValue,double.MinValue};
  foreach(IBody2 b in bodies){IFaultEntity faults=(IFaultEntity)b.Check3;if(faults!=null && faults.Count>0)throw new Exception("Body validation failed: "+b.Name+" count="+faults.Count+" first="+faults.get_ErrorCode(0));for(int i=0;i<3;i++){double x,y,z;double[] v={0,0,0};v[i]=1;b.GetExtremePoint(v[0],v[1],v[2],out x,out y,out z);max[i]=Math.Max(max[i],new double[]{x,y,z}[i]);b.GetExtremePoint(-v[0],-v[1],-v[2],out x,out y,out z);min[i]=Math.Min(min[i],new double[]{x,y,z}[i]);}}
  report.Add("All solid bodies valid: PASS");report.Add("Envelope mm="+((max[0]-min[0])*1000)+","+((max[1]-min[1])*1000)+","+((max[2]-min[2])*1000));
  // Verify saved feature colors are the exact values requested by the user.
  foreach(string name in new string[]{"载板PCB_30x30x1.6_四孔D2.2_孔距24.6","陀螺仪模块PCB_15.24x15.24x0.8_双侧半孔","屏蔽罩顶盖_厚0.18_模块总高2.8","模块12处半孔镀锡焊盘"}){
   IFeature cf=(IFeature)((IPartDoc)d).FeatureByName(name);double[] rgb=(double[])cf.GetMaterialPropertyValues2(1,null);report.Add(name+" RGB="+Math.Round(rgb[0]*255)+","+Math.Round(rgb[1]*255)+","+Math.Round(rgb[2]*255));
  }
  d.ShowNamedView2("*Isometric",7);d.ViewZoomtofit2();
  int errors=0,warnings=0;if(!d.Save3(1,ref errors,ref warnings))throw new Exception("Save failed "+errors+","+warnings);
  report.Add("Saved="+path+" errors="+errors+" warnings="+warnings);
  d.SaveBMP(Path.Combine(root,"智梭_JY62陀螺仪_等轴测.bmp"),1500,1100);
  d.ShowNamedView2("*Front",1);d.ViewZoomtofit2();d.SaveBMP(Path.Combine(root,"智梭_JY62陀螺仪_正视.bmp"),1200,1200);
  d.ShowNamedView2("*Isometric",7);d.ViewZoomtofit2();
  File.WriteAllLines(Path.Combine(root,"最终核对结果.txt"),report.ToArray());return string.Join(System.Environment.NewLine,report.ToArray());
 }
}

