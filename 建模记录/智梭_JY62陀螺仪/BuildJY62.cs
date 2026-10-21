using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using SolidWorks.Interop.sldworks;

public class BuildJY62 {
 static string root=@"D:\Robot\2027工创赛智梭速运升降台\建模记录\智梭_JY62陀螺仪";
 static string output=@"D:\Robot\2027工创赛智梭速运升降台\模型\智梭_JY62陀螺仪.SLDPRT";
 static IModelDoc2 doc; static IFeature plane; static ISketchManager sm;
 static List<string> report=new List<string>();
 static double[] green=Color(16,56,32),silver=Color(174,186,198),black=Color(28,31,34),ceramic=Color(129,119,94),ink=Color(38,47,52);
 static double[] Color(int r,int g,int b){return new double[]{r/255.0,g/255.0,b/255.0,0.35,0.75,0.25,0.25,0,0};}
 static void Log(string s){report.Add(s); File.AppendAllText(Path.Combine(root,"build_progress.txt"),s+System.Environment.NewLine);}
 static void Start(){doc.ClearSelection2(true);if(!plane.Select2(false,0))throw new Exception("Select plane");sm.InsertSketch(true);sm.AddToDB=true;sm.DisplayWhenAdded=false;}
 static void Line(double x,double y,double a,double b){sm.CreateLine(x/1000,y/1000,0,a/1000,b/1000,0);}
 static void Arc(double x,double y,double sx,double sy,double ex,double ey,short dir){sm.CreateArc(x/1000,y/1000,0,sx/1000,sy/1000,0,ex/1000,ey/1000,0,dir);}
 static void Rect(double x,double y,double w,double h){sm.CreateCornerRectangle((x-w/2)/1000,(y-h/2)/1000,0,(x+w/2)/1000,(y+h/2)/1000,0);}
 static void Circle(double x,double y,double r){sm.CreateCircleByRadius(x/1000,y/1000,0,r/1000);}
 static void Round(double x,double y,double w,double h,double r){
  double l=x-w/2,ri=x+w/2,b=y-h/2,t=y+h/2;
  Line(l+r,b,ri-r,b);Arc(ri-r,b+r,ri-r,b,ri,b+r,1);Line(ri,b+r,ri,t-r);Arc(ri-r,t-r,ri,t-r,ri-r,t,1);
  Line(ri-r,t,l+r,t);Arc(l+r,t-r,l+r,t,l,t-r,1);Line(l,t-r,l,b+r);Arc(l+r,b+r,l,b+r,l+r,b,1);
 }
 static IFeature Finish(string name,double z,double h,double[] color){
  sm.AddToDB=false;sm.DisplayWhenAdded=true;
  ISketch sk=(ISketch)doc.GetActiveSketch2(); IFeature sf=(IFeature)sk; sf.Name=name+"_草图";
  sm.InsertSketch(true);doc.ClearSelection2(true);sf.Select2(false,0);
  IFeature f=doc.FeatureManager.FeatureExtrusion2(true,false,false,0,0,h/1000,h/1000,false,false,false,false,0,0,false,false,false,false,false,true,true,z==0?0:3,z/1000,false);
  if(f==null)throw new Exception("Extrusion failed: "+name);
  f.Name=name;f.SetMaterialPropertyValues2(color,1,null);doc.ClearSelection2(true);Log("Created "+name);return f;
 }
 static void CastellatedOutline(double cy){
  double a=7.62,r=.45,b=cy-a,t=cy+a;
  Line(-a,b,a,b);double last=b;
  for(int i=0;i<6;i++){double yy=cy-6.35+i*2.54;Line(a,last,a,yy-r);Arc(a,yy,a,yy-r,a,yy+r,-1);last=yy+r;}
  Line(a,last,a,t);Line(a,t,-a,t);last=t;
  for(int i=5;i>=0;i--){double yy=cy-6.35+i*2.54;Line(-a,last,-a,yy+r);Arc(-a,yy,-a,yy+r,-a,yy-r,-1);last=yy-r;}
  Line(-a,last,-a,b);
 }
 static void Pad(int side,double y){
  double x=side*7.62,inner=side*5.72,r=.45,h=.82;
  if(side==1){Line(inner,y-h,x,y-h);Line(x,y-h,x,y-r);Arc(x,y,x,y-r,x,y+r,-1);Line(x,y+r,x,y+h);Line(x,y+h,inner,y+h);Line(inner,y+h,inner,y-h);}
  else {Line(inner,y-h,inner,y+h);Line(inner,y+h,x,y+h);Line(x,y+h,x,y+r);Arc(x,y,x,y+r,x,y-r,-1);Line(x,y-r,x,y-h);Line(x,y-h,inner,y-h);}
 }
 static void Chip(string name,double x,double y,double w,double h,double height,double[] color){Start();Rect(x,y,w,h);Finish(name,1.64,height,color);}
 static void TextMark(string name,string text,double x,double y,double z,double height,double[] color){
  Start();sm.AddToDB=false;
  ISketchText st=(ISketchText)doc.InsertSketchText(x/1000,y/1000,0,text,0,0,0,100,100);
  if(st==null)throw new Exception("Sketch text failed "+name);
  ITextFormat tf=(ITextFormat)st.GetTextFormat();tf.CharHeight=height/1000;tf.TypeFaceName="Arial";st.SetTextFormat(false,tf);
  Finish(name,z,.012,color);
 }
 public static string Run(){
  if(File.Exists(output))throw new Exception("Refusing to overwrite existing part");
  Log("Start "+DateTime.Now.ToString("o"));
  ISldWorks sw=(ISldWorks)Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application"));sw.Visible=true;
  doc=(IModelDoc2)sw.NewDocument(@"C:\ProgramData\SOLIDWORKS\SOLIDWORKS 2026\templates\gb_part.prtdot",0,0,0);
  if(doc==null)throw new Exception("NewDocument failed");
  doc.SetUserPreferenceIntegerValue(263,0);sm=doc.SketchManager;
  plane=(IFeature)doc.FirstFeature();while(plane!=null && plane.GetTypeName2()!="RefPlane")plane=(IFeature)plane.GetNextFeature();
  if(plane==null)throw new Exception("No reference plane");
  doc.MaterialPropertyValues=green;
  Start();Round(0,0,30,30,1.5);foreach(double x in new double[]{-12.3,12.3})foreach(double y in new double[]{-12.3,12.3})Circle(x,y,1.1);
  Finish("载板PCB_30x30x1.6_四孔D2.2_孔距24.6",0,1.6,green);
  Start();foreach(double x in new double[]{-12.3,12.3})foreach(double y in new double[]{-12.3,12.3}){Circle(x,y,1.7);Circle(x,y,1.1);}Finish("载板安装孔镀锡环",1.6,.035,silver);
  Start();foreach(int side in new int[]{-1,1})for(int i=0;i<6;i++)Rect(side*6.85,2.5-6.35+i*2.54,2.5,1.65);Finish("载板12处焊接焊盘_间距2.54",1.6,.1,silver);
  Start();CastellatedOutline(2.5);Finish("陀螺仪模块PCB_15.24x15.24x0.8_双侧半孔",1.7,.8,green);
  Start();foreach(int side in new int[]{-1,1})for(int i=0;i<6;i++)Pad(side,2.5-6.35+i*2.54);Finish("模块12处半孔镀锡焊盘",2.5,.035,silver);
  // Hollow metal shield, 0.18 mm walls and a 0.18 mm lid; total module height 2.8 mm.
  Start();Round(0,2.5,10.8,14.4,.28);Round(0,2.5,10.44,14.04,.1);Finish("屏蔽罩侧壁_厚0.18",2.5,1.82,silver);
  Start();Round(0,2.5,10.8,14.4,.28);Finish("屏蔽罩顶盖_厚0.18_模块总高2.8",4.32,.18,silver);
  Start();Circle(4.2,-2.8,.32);Finish("屏蔽罩定位标记",4.5,.012,ink);
  // Simplified carrier components visible in the reference installation illustration.
  Chip("载板控制芯片_4x4",2.4,-10,3.6,3.6,.8,black);
  Start();for(int i=0;i<6;i++){double a=2.4-1.25+i*.5;Rect(a,-12.05,.22,.55);Rect(a,-7.95,.22,.55);double b=-10-1.25+i*.5;Rect(.35,b,.55,.22);Rect(4.45,b,.55,.22);}Finish("载板控制芯片24引脚",1.6,.17,silver);
  double[,] passives=new double[,]{{-8.8,-9.6},{-8.8,-6.7},{-5.2,-11},{8.9,-7.2},{9,-10.2},{-10,3.5}};
  Start();for(int i=0;i<passives.GetLength(0);i++){Rect(passives[i,0]-.72,passives[i,1],.6,1);Rect(passives[i,0]+.72,passives[i,1],.6,1);}Finish("载板贴片元件焊盘",1.6,.06,silver);
  for(int i=0;i<passives.GetLength(0);i++)Chip("贴片元件_"+(i+1),passives[i,0],passives[i,1],1.5,.8,.42,i%2==0?ceramic:black);
  Start();for(int i=0;i<passives.GetLength(0);i++){Rect(passives[i,0]-.68,passives[i,1],.3,.8);Rect(passives[i,0]+.68,passives[i,1],.3,.8);}Finish("贴片元件端电极",2.06,.025,silver);
  ICustomPropertyManager cp=doc.Extension.get_CustomPropertyManager("");
  cp.Add3("模块尺寸",30,"15.24 x 15.24 x 2.8 mm（按参考图）",2);
  cp.Add3("载板尺寸",30,"30 x 30 x 1.6 mm（参考图未标注，按比例示意）",2);
  cp.Add3("颜色",30,"PCB #103820；屏蔽罩及焊盘 #AEBAC6",2);
  cp.Add3("参考图说明",30,"参考图外罩印字为JY901S；按用户要求命名JY62。载板元件及安装孔为示意，不作电路生产依据。",2);
  doc.EditRebuild3();doc.ClearSelection2(true);doc.ShowNamedView2("*Isometric",7);doc.ViewZoomtofit2();
  object[] bodies=(object[])((IPartDoc)doc).GetBodies2(0,false);Log("SolidBodies="+bodies.Length);
  double[] min={double.MaxValue,double.MaxValue,double.MaxValue},max={double.MinValue,double.MinValue,double.MinValue};
  foreach(IBody2 body in bodies){
   foreach(IFace2 face in (object[])body.GetFaces()){} // Force geometry access before saving.
   for(int i=0;i<3;i++){double x,y,z;double[] v={0,0,0};v[i]=1;body.GetExtremePoint(v[0],v[1],v[2],out x,out y,out z);max[i]=Math.Max(max[i],new double[]{x,y,z}[i]);body.GetExtremePoint(-v[0],-v[1],-v[2],out x,out y,out z);min[i]=Math.Min(min[i],new double[]{x,y,z}[i]);}
  }
  Log("ExactEnvelopeMM="+((max[0]-min[0])*1000).ToString("F4",CultureInfo.InvariantCulture)+","+((max[1]-min[1])*1000).ToString("F4",CultureInfo.InvariantCulture)+","+((max[2]-min[2])*1000).ToString("F4",CultureInfo.InvariantCulture));
  IFeature f=(IFeature)doc.FirstFeature();while(f!=null){int err=f.GetErrorCode();Log("Feature="+f.Name+" | "+f.GetTypeName2()+" | Error="+err);if(err!=0)throw new Exception("Feature error "+f.Name+" "+err);f=(IFeature)f.GetNextFeature();}
  int errors=0,warnings=0;if(!doc.Extension.SaveAs(output,0,1,null,ref errors,ref warnings))throw new Exception("Save failed "+errors+" "+warnings);
  Log("Saved="+output+" SaveErrors="+errors+" SaveWarnings="+warnings);
  doc.SaveBMP(Path.Combine(root,"智梭_JY62陀螺仪_等轴测.bmp"),1500,1100);
  doc.ShowNamedView2("*Front",1);doc.ViewZoomtofit2();doc.SaveBMP(Path.Combine(root,"智梭_JY62陀螺仪_正视.bmp"),1200,1200);
  doc.ShowNamedView2("*Isometric",7);doc.ViewZoomtofit2();
  File.WriteAllLines(Path.Combine(root,"核对结果.txt"),report.ToArray());return string.Join(System.Environment.NewLine,report.ToArray());
 }
}
