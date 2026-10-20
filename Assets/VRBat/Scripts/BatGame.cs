using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem;
namespace VRBat {
public partial class BatGame:MonoBehaviour {
 public StageView view; public bool workshop,left,integratedMode; public float height=1.65f; public float roundDuration=90f; public global::GamePlayModule integratedGameplay;
 public string phase="menu"; public double now,phaseAt,runAt,nextAt; public int score,index,introIndex,cycle,sectionStep,lastSection=-1,ballSourceStep; [NonSerialized] public Challenge current,incoming;
 public Vector3 grip=new Vector3(.28f,1.05f,.2f),axis=new Vector3(-.25f,.6f,.759934f); public float dy;
 public bool paused; public string evidence; StreamWriter log; int frames;double frameSeconds;float minFps=999; public int[] presented=new int[8],success=new int[8];public int bulletHits,bulletTotal,bandHits;public double bandHintUntil;
 public readonly string[] sequenceA={"ball","cake","poison","boomerang","magic","bomb","bullets","chest","magic","ball","magic","ball"},sequenceB={"ball","cake","ball","bomb","bullets","ball","chest","ball"};
 public readonly string[] introA={"ball","cake"},introB={"ball","cake","bomb","bomb","bullets","chest"};
 float reviewYaw,reviewPitch=6; public int pitchSerial; System.Random pitchRandom; public int NextPitchVariant(){pitchSerial++;return pitchRandom.Next(1,8192);} public bool suite; public int magicStep; public bool captureSequence; public Transform targets; public Effects fx; GUIStyle label,button;readonly HashSet<string> captured=new HashSet<string>();bool capture,quitAfter,finishing;int maxCycles=2;double captureAt;string pendingCapture;int trialFailures; public int introFailures; public bool introAssisted; double introDemoUntil; bool testIntroFailure,testIntroOnly,testMissStreak,testBatRegions; readonly int[] aMissStreak=new int[8]; public Vector3 oldGrip,oldAxis;float headPitch=10,headYaw;float accumulator;Vector3 xrGrip,xrAxis;float xrCalibrationTime,xrHeightSum,measuredHeight=1.65f,manualHeightOffset; bool lastTrigger; LineRenderer pointer;GameObject skipUi,pauseUi,heightUi;UnityEngine.UI.Text heightText;
 VRMenuRay leftMenuRay,rightMenuRay; LineRenderer rightPointer;
 public float Mirror=>left?-1:1;public Vector3 W(Vector3 p)=>new Vector3(p.x*Mirror,p.y+dy,p.z);public Vector3 D(Vector3 p)=>new Vector3(p.x*Mirror,p.y,p.z);
 public double Remaining=>Math.Max(0,roundDuration-(now-runAt));
 System.Collections.IEnumerator Start(){
 if(Application.platform==RuntimePlatform.WindowsPlayer){var settings=UnityEngine.XR.Management.XRGeneralSettings.Instance;if(settings&&settings.Manager&&!settings.Manager.isInitializationComplete){yield return settings.Manager.InitializeLoader();if(settings.Manager.activeLoader)settings.Manager.StartSubsystems();}}
var args=Environment.GetCommandLineArgs();workshop=false;suite=args.Contains("--suite");captureSequence=args.Contains("--record");capture=args.Contains("--capture");quitAfter=args.Contains("--verify");testIntroFailure=args.Contains("--test-intro-failure");testIntroOnly=args.Contains("--verify-intro");testMissStreak=args.Contains("--test-miss-streak");testBatRegions=args.Contains("--test-bat-regions");left=args.Contains("--test-left");maxCycles=Mathf.Clamp(int.Parse(Arg(args,"--cycles","2")),1,2);var testHeight=Arg(args,"--test-height",null);if(testHeight!=null){height=float.Parse(testHeight,System.Globalization.CultureInfo.InvariantCulture);measuredHeight=height;xrCalibrationTime=.5f;}reviewYaw=float.Parse(Arg(args,"--test-yaw","0"),System.Globalization.CultureInfo.InvariantCulture);reviewPitch=float.Parse(Arg(args,"--test-pitch","6"),System.Globalization.CultureInfo.InvariantCulture);pitchRandom=new System.Random(int.Parse(Arg(args,"--test-seed",Environment.TickCount.ToString())));magicStep=suite?0:pitchRandom.Next(3);evidence=Arg(args,"--evidence",Path.Combine(Application.persistentDataPath,"Evidence"));Directory.CreateDirectory(evidence);log=new StreamWriter(Path.Combine(evidence,(workshop?"B":"A")+"-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".csv")){AutoFlush=true};log.WriteLine("time,cycle,phase,task,kind,event,score,detail");
 transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);transform.localScale=Vector3.one;
 view.Build();if(captureSequence){var recorder=gameObject.AddComponent<SequenceRecorder>();recorder.game=this;}targets=new GameObject("Live targets").transform;fx=gameObject.AddComponent<Effects>();fx.game=this;SetupSlowMotion();impactView=ImpactView.Create(this);Application.targetFrameRate=XRSettings.isDeviceActive?-1:60;QualitySettings.vSyncCount=0;if(integratedMode)ResetIntegrated();else ResetGame(workshop);}
 string Arg(string[] a,string key,string fallback){int i=Array.IndexOf(a,key);return i>=0&&i+1<a.Length?a[i+1]:fallback;}
 public void Event(string what,string detail="",string pitchKind=null){log?.WriteLine($"{now:F5},{cycle},{phase},{index},{pitchKind??current?.kind},{what},{score},{detail}");}
 public void Snap(string name,float delay=0){if(!capture)return;var key=(workshop?"B":"A")+"-"+cycle+"-"+phase+"-"+name;if(captured.Contains(key))return;captured.Add(key);pendingCapture=key;captureAt=now+delay;}
 public void ResetGame(bool b){ResetManual();ResetPresentation();if(impactView)impactView.Clear();b=false;paused=false;if(skipUi)Destroy(skipUi);if(pauseUi)Destroy(pauseUi);if(heightUi)Destroy(heightUi);if(fx)fx.ResetEffects();if(current!=null)current.Destroy();if(incoming!=null)incoming.Destroy();current=incoming=null;foreach(Transform t in targets)Destroy(t.gameObject);if(view.menu)Destroy(view.menu);if(view.results)Destroy(view.results);workshop=b;sectionStep=ballSourceStep=0;lastSection=-1;score=index=introIndex=trialFailures=bandHits=bulletHits=bulletTotal=0;introAssisted=false;introDemoUntil=0;Array.Clear(aMissStreak,0,aMissStreak.Length);Array.Clear(presented,0,8);Array.Clear(success,0,8);dy=height-1.65f;view.Calibrate(dy,left);phase="menu";phaseAt=now;view.hud.SetActive(false);view.Lamps(0);grip=W(new Vector3(.28f,1.05f,.2f));axis=D(new Vector3(-.25f,.6f,.759934f));ShowMenu();Event("reset");Snap("menu",1);}
 void ShowMenu(){var p=view.Panel("Menu",W(new Vector3(0,1.38f,1.8f)),new Vector2(1.2f,.70f),transform);view.menu=p.gameObject;view.Label(p,"バット・カーニバル",new Vector2(0,.23f),new Vector2(1.14f,.1f),.075f);view.Button(p,"右手",new Vector2(-.22f,.04f),new Vector2(.30f,.12f),()=>{left=false;ResetGame(workshop);});view.Button(p,"左手",new Vector2(.22f,.04f),new Vector2(.30f,.12f),()=>{left=true;ResetGame(workshop);});view.Check(p,new Vector2(left?.09f:-.35f,.04f));view.Button(p,"体験する",new Vector2(-.26f,-.19f),new Vector2(.40f,.14f),BeginIntro);view.Button(p,"開始",new Vector2(.26f,-.19f),new Vector2(.40f,.14f),Countdown);if(xrCalibrationTime>=.5f)ShowHeightAdjustment();}
 void ShowIntroSkip(){if(skipUi)Destroy(skipUi);var p=view.Panel("Intro skip",W(new Vector3(1.65f,2.04f,3.35f)),new Vector2(.38f,.16f),transform);skipUi=p.gameObject;view.Button(p,"開始",Vector2.zero,new Vector2(.38f,.16f),Countdown);}
 void ShowHeightAdjustment(){if(heightUi)Destroy(heightUi);if(phase!="menu"||measuredHeight>=1.35f&&measuredHeight<=1.90f)return;var p=view.Panel("Height adjustment",W(new Vector3(0,.91f,1.8f)),new Vector2(1.15f,.31f),transform);heightUi=p.gameObject;heightText=view.Label(p,"高さを合わせる",new Vector2(0,.085f),new Vector2(1.05f,.08f),.05f);view.Button(p,"−1cm",new Vector2(-.27f,-.065f),new Vector2(.35f,.10f),()=>AdjustHeight(-.01f));view.Button(p,"＋1cm",new Vector2(.27f,-.065f),new Vector2(.35f,.10f),()=>AdjustHeight(.01f));UpdateHeightLabel();}
 void UpdateHeightLabel(){if(heightText)heightText.text=$"高さを合わせる  {manualHeightOffset*100:+0;-0;0}cm";}
 void AdjustHeight(float delta){manualHeightOffset=Mathf.Clamp(Mathf.Round((manualHeightOffset+delta)*100)/100f,-.20f,.20f);height=measuredHeight+manualHeightOffset;dy=height-1.65f;view.Calibrate(dy,left);if(heightUi)heightUi.transform.position=W(new Vector3(0,.91f,1.8f));UpdateHeightLabel();Event("height_adjust",manualHeightOffset.ToString("F2"));}
 public void BeginIntro(){if(view.menu)Destroy(view.menu);if(heightUi)Destroy(heightUi);introIndex=introFailures=0;introAssisted=false;introDemoUntil=0;ShowIntroSkip();phase="intro";phaseAt=now;nextAt=now;Event("intro");}
 public void Countdown(){if(view.menu)Destroy(view.menu);if(skipUi)Destroy(skipUi);if(heightUi)Destroy(heightUi);fx.CancelDemonstration();introAssisted=false;if(current!=null){current.Destroy();current=null;}phase="countdown";phaseAt=now;view.hud.SetActive(true);view.Lamps(3);Event("countdown");}
 void BeginRun(){PrepareManualRound();view.hud.SetActive(true);phase="play";runAt=now;index=0;score=0;Array.Clear(aMissStreak,0,aMissStreak.Length);Array.Clear(presented,0,8);Array.Clear(success,0,8);bandHits=bulletHits=bulletTotal=0;nextAt=now;view.Lamps(0);Event("start");if(testIntroOnly){Event("verification_end","intro_only");finishing=true;Application.Quit();}}
 void Results(){phase="results";phaseAt=now;view.hud.SetActive(false);fx.Finale();Event("results",$"points={score};hits={string.Join("/",success)};fps={frames/Math.Max(.001,frameSeconds):F2}");if(integratedMode)return;var p=view.Panel("Results",W(new Vector3(0,1.75f,2.1f)),new Vector2(1.5f,.8f),transform);view.results=p.gameObject;view.Label(p,"結果",new Vector2(0,.29f),new Vector2(1.3f,.08f),.065f);view.Label(p,score.ToString(),new Vector2(0,.13f),new Vector2(1.2f,.18f),.14f);
 if(workshop){string[] names={"球","ケーキ","爆弾","弾","箱"};for(int i=0;i<5;i++){float x=-.56f+i*.28f;view.Label(p,names[i],new Vector2(x,-.08f),new Vector2(.27f,.06f),.045f);view.Label(p,i==3?$"{bulletHits}/{bulletTotal}":$"{success[i]}/{presented[i]}",new Vector2(x,-.18f),new Vector2(.27f,.06f),.045f);if(i==3)view.Label(p,$"全弾 {success[i]}/{presented[i]}",new Vector2(x,-.255f),new Vector2(.27f,.06f),.036f);}}else view.Label(p,$"打てた数 {success.Sum()}  ／  帯の数 {bandHits}",new Vector2(0,-.13f),new Vector2(1.4f,.1f),.065f);view.Button(p,"もう一度",new Vector2(0,-.33f),new Vector2(.42f,.10f),()=>ResetGame(false));Snap("results",.5f);}
 void Update(){if(view.eye==null)return;float dt=Time.unscaledDeltaTime;frames++;frameSeconds+=dt;minFps=Mathf.Min(minFps,1/Mathf.Max(dt,.00001f));if(Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame){if(paused)Resume();else{paused=true;Event("pause");}}
 if(desktopInput)ReadDesktop(dt);else ReadXR(dt);
 if(Mouse.current!=null&&Mouse.current.leftButton.wasPressedThisFrame)ClickRay(view.eye.ScreenPointToRay(Mouse.current.position.ReadValue()));
 if(paused&&!pauseUi){var pp=view.Panel("Pause",W(new Vector3(0,1.5f,1.5f)),new Vector2(.5f,.2f),transform);pauseUi=pp.gameObject;view.Button(pp,"続ける",Vector2.zero,new Vector2(.5f,.2f),Resume);}if(!paused&&pauseUi)Destroy(pauseUi);
 if(!paused){accumulator+=dt;var beforeGrip=grip;var beforeAxis=axis;float consumed=0;while(accumulator>=1f/240){consumed+=1f/240;{oldGrip=grip;oldAxis=axis;grip=Vector3.Lerp(beforeGrip,xrGrip,Mathf.Clamp01(consumed/dt));axis=Vector3.Slerp(beforeAxis,xrAxis,Mathf.Clamp01(consumed/dt));}Tick(1f/240);accumulator-=1f/240;}grip=xrGrip;axis=xrAxis;}
 if(view.bat.gameObject.activeSelf!=(phase!="menu"))view.bat.gameObject.SetActive(phase!="menu");view.bat.SetPositionAndRotation(grip,Quaternion.FromToRotation(Vector3.up,axis));view.score.text=score.ToString();view.time.text=phase=="play"?(Remaining>0?Math.Ceiling(Remaining).ToString():"残りの球"):"90";view.time.fontSize=phase=="play"&&Remaining<=0?90:138;
 view.band.color=BatGeometry.ColorOf(now<bandHintUntil?"#FFE28A":"#F6C850");
 UpdateManualVisuals();
 SyncPresentationFrame();
 if(pendingCapture!=null&&now>=captureAt){ScreenCapture.CaptureScreenshot(Path.Combine(evidence,pendingCapture+".png"));Event("screenshot",pendingCapture);pendingCapture=null;}
 }
 public int SectionIndex(float elapsed){int section=elapsed<30?0:elapsed<60?1:2;if(section!=lastSection){lastSection=section;sectionStep=0;}return sectionStep++;}
 void Tick(float dt){
  if(finishing)return;now+=dt;AdvancePresentationClock(dt);
  if(phase=="countdown"){view.Lamps(3-(int)(now-phaseAt));if(now-phaseAt>=3)BeginRun();return;}
  if(phase=="play")TickManual(dt);
 }
 public void RecordAResult(string kind,bool ok){int i=KindIndex(kind);aMissStreak[i]=ok?0:Mathf.Min(2,aMissStreak[i]+1);}
 public int KindIndex(string kind){string[] kinds={"ball","cake","poison","boomerang","magic","bomb","bullets","chest"};return Mathf.Max(0,Array.IndexOf(kinds,kind));}
 public void Award(int points,Vector3 p,bool sweet=false){score+=points;if(sweet)bandHits++;fx.Points(p,points);Event("points",points.ToString());}
 void ReadXR(float dt){var head=UnityEngine.XR.InputDevices.GetDeviceAtXRNode(XRNode.Head);var hand=UnityEngine.XR.InputDevices.GetDeviceAtXRNode(left?XRNode.LeftHand:XRNode.RightHand);bool playing=phase=="play"||phase=="intro";bool headTracked=head.TryGetFeatureValue(UnityEngine.XR.CommonUsages.isTracked,out bool ht)&&ht;bool handTracked=hand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.isTracked,out bool tracked)&&tracked;bool hasHeadPosition=head.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition,out var hp),hasHeadRotation=head.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation,out var hr);if(playing&&(!headTracked||!handTracked||!hasHeadPosition||!hasHeadRotation))paused=true;if(hasHeadPosition&&hasHeadRotation){view.eye.transform.SetPositionAndRotation(hp,hr);if(headTracked&&xrCalibrationTime<.5f){xrCalibrationTime+=dt;xrHeightSum+=hp.y*dt;measuredHeight=xrHeightSum/xrCalibrationTime;height=measuredHeight+manualHeightOffset;dy=height-1.65f;view.Calibrate(dy,left);if(xrCalibrationTime>=.5f)ShowHeightAdjustment();}}
 // The controller's forward axis runs from the grip toward the bat tip.
 // The rendered bat and contact calculations both use this same longitudinal axis.
 if(hand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition,out var p)&&hand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation,out var r)){xrGrip=p;xrAxis=r*Vector3.forward;}else if(phase=="play"||phase=="intro")paused=true;
 bool show=phase=="menu"||phase=="intro"||phase=="results"||paused;
 UpdateMenuHand(XRNode.LeftHand,false,show,ref leftMenuRay,ref pointer);
 UpdateMenuHand(XRNode.RightHand,true,show,ref rightMenuRay,ref rightPointer);
 }
 void UpdateMenuHand(XRNode node,bool right,bool show,ref VRMenuRay menuRay,ref LineRenderer line){
 var device=UnityEngine.XR.InputDevices.GetDeviceAtXRNode(node);
 bool tracked=device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.isTracked,out bool tracking)&&tracking;
 if(show&&tracked&&device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition,out var position)&&device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation,out var rotation)){
 if(!menuRay)menuRay=VRMenuRay.Create(transform,right);
 menuRay.SetPose(position,rotation,true);
 if(!line)line=BatGeometry.Line(right?"Right menu pointer":"Left menu pointer",new[]{position,position+rotation*Vector3.forward*2},.003f,view.teal,transform);
 line.gameObject.SetActive(true);line.SetPosition(0,position);line.SetPosition(1,position+rotation*Vector3.forward*2);
 }else {if(menuRay)menuRay.gameObject.SetActive(false);if(line)line.gameObject.SetActive(false);}
 }
 void Resume(){grip=xrGrip;axis=xrAxis;oldGrip=xrGrip;oldAxis=xrAxis;accumulator=0;paused=false;Event("resume");}
 void ClickRay(Ray ray){foreach(var hit in Physics.RaycastAll(ray,10).OrderBy(x=>x.distance)){var b=hit.collider.GetComponent<WorldButton>();if(b){b.Click();fx.Sound("select_001",-20,1);break;}}}
 static float Smooth(float x){x=Mathf.Clamp01(x);return x*x*x*(x*(x*6-15)+10);}
 void OnGUI(){if(view==null||view.font==null||view.eye==null)return;if(label==null){label=new GUIStyle(GUI.skin.label){font=view.font,fontSize=26};button=new GUIStyle(GUI.skin.button){font=view.font,fontSize=22};}if(XRSettings.isDeviceActive)return;ManualControlsGUI();if(paused){if(GUI.Button(new Rect(Screen.width/2-90,Screen.height/2-25,180,50),"続ける",button))Resume();}
 if(phase=="menu"){GUI.Label(new Rect(32,Screen.height-95,120,36),"音量",label);AudioListener.volume=GUI.HorizontalSlider(new Rect(32,Screen.height-50,180,24),AudioListener.volume,0,1);}
 if(phase=="menu"&&!integratedMode){if(GUI.Button(new Rect(Screen.width/2-150,Screen.height/2,140,45),"体験する",button))BeginIntro();if(GUI.Button(new Rect(Screen.width/2+10,Screen.height/2,140,45),"開始",button))Countdown();}if(phase=="intro"&&GUI.Button(new Rect(Screen.width-150,100,110,42),"開始",button))Countdown();}
 void OnDestroy(){log?.Dispose();}
}
}
