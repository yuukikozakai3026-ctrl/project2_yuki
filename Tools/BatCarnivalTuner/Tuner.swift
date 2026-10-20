import SwiftUI
import AppKit
import UniformTypeIdentifiers

struct Settings: Codable, Equatable {
    var schemaVersion = 1, seed = 20261006
    var roundSeconds = 60.0, intervalStart = 2.2, intervalEnd = 0.95, intervalJitter = 0.18
    var bombPercentStart = 4.0, bombPercentEnd = 10.0, bombPenalty = 50.0, stunSeconds = 1.5
    var minimumPoints = 5.0, maximumPoints = 100.0, maximumScoreSpeed = 8.0, bandSpeedBonus = 0.5, chargeSeconds = 1.0
    var zoneWidth = 1.1, zoneHeight = 0.65, zoneCenterY = 1.3, flightSeconds = 2.4, arcHeight = 0.65, launchDistance = 7.75
    var ballGap = 0.30, bombGap = 0.65, clearance = 0.32, bombClearance = 0.50
    mutating func validate() {
        func safe(_ n: Double, _ lo: Double, _ hi: Double, _ fallback: Double) -> Double { min(hi,max(lo,n.isFinite ? n : fallback)) }
        roundSeconds=safe(roundSeconds,10,300,60); intervalStart=safe(intervalStart,0.4,8,2.2); intervalEnd=safe(intervalEnd,0.4,intervalStart,0.95)
        intervalJitter=safe(intervalJitter,0,0.5,0.18); bombPercentStart=safe(bombPercentStart,0,100,4);bombPercentEnd=safe(bombPercentEnd,0,100,10)
        bombPenalty=safe(bombPenalty,0,1000,50);stunSeconds=safe(stunSeconds,0,10,1.5)
        minimumPoints=safe(minimumPoints,0,1000,5);maximumPoints=safe(maximumPoints,minimumPoints,5000,100)
        maximumScoreSpeed=safe(maximumScoreSpeed,0.5,30,8);bandSpeedBonus=safe(bandSpeedBonus,0,10,0.5);chargeSeconds=safe(chargeSeconds,0.2,3,1)
        zoneWidth=safe(zoneWidth,0.3,1.8,1.1);zoneHeight=safe(zoneHeight,0.2,1.1,0.65);zoneCenterY=safe(zoneCenterY,0.8,1.7,1.3)
        flightSeconds=safe(flightSeconds,1,6,2.4);arcHeight=safe(arcHeight,0.05,2,0.65);launchDistance=safe(launchDistance,4,16,7.75)
        ballGap=safe(ballGap,0.15,2,0.3);bombGap=safe(bombGap,ballGap,3,0.65);clearance=safe(clearance,0.2,1,0.32);bombClearance=safe(bombClearance,clearance,1.5,0.5)
    }
    func progress(_ t: Double) -> Double { let u=min(1,max(0,t/roundSeconds));return u*u*(3-2*u) }
    func interval(_ t: Double) -> Double { intervalStart+(intervalEnd-intervalStart)*progress(t) }
    func bombRate(_ t: Double) -> Double { bombPercentStart+(bombPercentEnd-bombPercentStart)*progress(t) }
    func points(_ speed: Double, _ band: Bool) -> Int { Int((minimumPoints+(maximumPoints-minimumPoints)*min(1,max(0,(speed+(band ? bandSpeedBonus:0))/maximumScoreSpeed))).rounded()) }
}
struct V3 { var x: Double,y: Double,z: Double
    func distance(_ p: V3) -> Double { sqrt(pow(x-p.x,2)+pow(y-p.y,2)+pow(z-p.z,2)) }
}
struct Pitch: Identifiable {
    var id: Int, bomb: Bool, launch: Double, seconds: Double, arc: Double, source: V3, end: V3
    var arrival: Double {launch+seconds}
    func at(_ time: Double) -> V3 {
        let t=min(1,max(0,(time-launch)/seconds)),u=t+0.24*sin(.pi*t)
        return V3(x:source.x+(end.x-source.x)*u,y:source.y+(end.y-source.y)*u+4*arc*u*(1-u),z:source.z+(end.z-source.z)*u)
    }
}
struct Generator {
    var state: UInt32
    mutating func next() -> Double {state=state &* 1664525 &+ 1013904223; return Double(state >> 8)/16777216}
    static func build(_ c: Settings) -> [Pitch] {
        var result=[Pitch](), rng=Generator(state:UInt32(truncatingIfNeeded:c.seed)),time=0.8
        while time<c.roundSeconds {
            let bomb=rng.next()*100<c.bombRate(time)
            var chosen: Pitch? = nil
            for _ in 0..<120 {
                for _ in 0..<24 {
                    let p=Pitch(id:result.count,bomb:bomb,launch:time,seconds:c.flightSeconds,arc:c.arcHeight,source:V3(x:0,y:1.71,z:c.launchDistance),end:V3(x:(rng.next()-0.5)*c.zoneWidth,y:c.zoneCenterY+(rng.next()-0.5)*c.zoneHeight,z:0.70))
                    if safe(p,result,c) {chosen=p;break}
                }
                if chosen != nil {break}
                time+=0.05
            }
            guard let p=chosen,time<c.roundSeconds else {break}
            result.append(p);time+=max(0.4,c.interval(time)*(1+(rng.next()*2-1)*c.intervalJitter))
        }
        return result
    }
    static func safe(_ p: Pitch,_ others: [Pitch],_ c: Settings) -> Bool {
        for q in others where q.arrival>=p.launch {
            let bomb=p.bomb || q.bomb, gap=bomb ? c.bombGap:c.ballGap,clear=bomb ? c.bombClearance:c.clearance
            if abs(p.arrival-q.arrival)<gap {return false}
            let stop=min(p.arrival,q.arrival);var t=p.launch
            while t<=stop+0.001 {if p.at(t).distance(q.at(t))<clear {return false};t+=0.025}
            if p.at(stop).distance(q.at(stop))<clear {return false}
        }
        return true
    }
}
@MainActor final class Model: ObservableObject {
    private var normalizing=false
    @Published var settings=Settings() { didSet {if !normalizing {normalizing=true;settings.validate();normalizing=false;dirty=true;rebuild()} } }
    @Published var pitches=[Pitch]()
    @Published var dirty=false
    @Published var message=""
    @Published var time=0.0
    @Published var playing=false
    @Published var tab="投球"
    let url=FileManager.default.homeDirectoryForCurrentUser.appendingPathComponent("Library/Application Support/VRBat/Bat Carnival/Tuning/settings.json")
    init() {
        if FileManager.default.fileExists(atPath:url.path) {
            do {var value=try JSONDecoder().decode(Settings.self,from:Data(contentsOf:url));value.validate();settings=value}
            catch {message="設定を読み込めませんでした。初期設定を表示しています。"}
        }
        dirty=false;rebuild()
    }
    func rebuild(){var c=settings;c.validate();pitches=Generator.build(c);time=min(time,c.roundSeconds)}
    func save(){
        settings.validate()
        do {try FileManager.default.createDirectory(at:url.deletingLastPathComponent(),withIntermediateDirectories:true);let encoder=JSONEncoder();encoder.outputFormatting=[.prettyPrinted,.sortedKeys];try encoder.encode(settings).write(to:url,options:.atomic);dirty=false;message="保存しました。次のゲーム開始時に反映されます。"}
        catch {message="保存できませんでした: \(error.localizedDescription)"}
    }
    func export(){let p=NSSavePanel();p.allowedContentTypes=[.json];p.nameFieldStringValue="ManualTuning.json";if p.runModal() == .OK,let target=p.url {do {let e=JSONEncoder();e.outputFormatting=[.prettyPrinted,.sortedKeys];try e.encode(settings).write(to:target,options:.atomic);message="設定を書き出しました。"}catch {message=error.localizedDescription}}}
    func importFile(){let p=NSOpenPanel();p.allowedContentTypes=[.json];p.allowsMultipleSelection=false;if p.runModal() == .OK,let target=p.url {do {var c=try JSONDecoder().decode(Settings.self,from:Data(contentsOf:target));c.validate();settings=c;message="読み込みました。ゲームへ反映するには保存してください。"}catch {message="読み込み失敗: \(error.localizedDescription)"}}}
}
struct SettingRow: View {
    var title: String,detail: String,unit: String,range: ClosedRange<Double>
    @Binding var value: Double
    var body: some View {
        VStack(alignment:.leading,spacing:6) {
            HStack {Text(title).fontWeight(.medium);Spacer();TextField("",value:$value,format:.number.precision(.fractionLength(0...2))).textFieldStyle(.roundedBorder).frame(width:72).multilineTextAlignment(.trailing);Text(unit).foregroundStyle(.secondary).frame(width:40,alignment:.leading)}
            Slider(value:$value,in:range)
            Text(detail).font(.caption).foregroundStyle(.secondary).fixedSize(horizontal:false,vertical:true)
        }.padding(.vertical,5)
    }
}
struct Card<Content: View>: View {
    var title: String,icon: String;@ViewBuilder var content: () -> Content
    var body: some View {VStack(alignment:.leading,spacing:12){Label(title,systemImage:icon).font(.title3.weight(.semibold));Divider();content()}.padding(20).frame(maxWidth:.infinity,alignment:.topLeading).background(Color(nsColor:.controlBackgroundColor),in:RoundedRectangle(cornerRadius:14)).overlay(RoundedRectangle(cornerRadius:14).stroke(.primary.opacity(0.08)))}
}
struct ContentView: View {
    @StateObject var model=Model()
    let tabs=["投球","打撃・爆弾","範囲・軌道","プレビュー"]
    var body: some View {
        VStack(spacing:0) {
            HStack(spacing:16){Image(systemName:"baseball.fill").font(.system(size:30)).foregroundStyle(.teal);VStack(alignment:.leading,spacing:4){Text("バット・カーニバル 調整").font(.title2.weight(.semibold));Text("保存した設定は次のゲーム開始時に反映されます").font(.subheadline).foregroundStyle(.secondary)};Spacer();if model.dirty {Text("未保存").font(.caption).foregroundStyle(.orange)};Button("ゲーム用に保存"){model.save()}.buttonStyle(.borderedProminent).tint(.teal).keyboardShortcut("s",modifiers:.command)}.padding(24)
            Picker("機能",selection:$model.tab){ForEach(tabs,id:\.self){Text($0).tag($0)}}.pickerStyle(.segmented).padding(.horizontal,24).padding(.bottom,18)
            Divider()
            ScrollView {
                VStack(spacing:18){
                    if model.tab=="投球" {pitchSettings}
                    if model.tab=="打撃・爆弾" {hitSettings}
                    if model.tab=="範囲・軌道" {spaceSettings}
                    if model.tab=="プレビュー" {preview}
                }.padding(24).frame(maxWidth:1200)
            }.id(model.tab)
            Divider()
            HStack {Text(model.message.isEmpty ? "設定とプレビューを確認してから保存してください" : model.message).font(.caption).foregroundStyle(.secondary).lineLimit(2);Spacer();Button("読み込み…"){model.importFile()};Button("書き出し…"){model.export()};Button("初期設定"){model.settings=Settings();model.message="初期設定に戻しました。保存するまでゲームの設定は変わりません。"}}
                .padding(.horizontal,24).padding(.vertical,12)
        }.frame(minWidth:900,minHeight:700).background(Color(nsColor:.windowBackgroundColor)).onReceive(Timer.publish(every:1/30,on:.main,in:.common).autoconnect()){_ in if model.playing {model.time+=1/30;if model.time>=model.settings.roundSeconds+model.settings.flightSeconds {model.time=model.settings.roundSeconds+model.settings.flightSeconds;model.playing=false}}}
    }
    func row(_ title:String,_ detail:String,_ key:WritableKeyPath<Settings,Double>,_ range:ClosedRange<Double>,_ unit:String) -> some View {SettingRow(title:title,detail:detail,unit:unit,range:range,value:Binding(get:{model.settings[keyPath:key]},set:{model.settings[keyPath:key]=$0}))}
    var pitchSettings: some View {
        HStack(alignment:.top,spacing:20){
            Card(title:"時間と投球ペース",icon:"clock") {
                row("プレイ時間","終了時に飛んでいる球は最後まで処理します。",\.roundSeconds,10...300,"秒")
                row("序盤の投球間隔","前の球が打たれる前でも次の球を投げます。",\.intervalStart,0.4...8,"秒")
                row("終盤の投球間隔","序盤から終盤へ滑らかに短くなります。",\.intervalEnd,0.4...max(0.4,model.settings.intervalStart),"秒")
                row("間隔のばらつき","±の割合。0.18 は ±18%です。",\.intervalJitter,0...0.5,"倍率")
            }
            Card(title:"爆弾の割合",icon:"flame") {
                row("序盤の爆弾率","全投球のうち爆弾になる確率です。",\.bombPercentStart,0...100,"%")
                row("終盤の爆弾率","途中の確率は滑らかに変化します。",\.bombPercentEnd,0...100,"%")
                Text("実際の個数は乱数で変わります。爆弾は到達タイミングと軌道の間隔を広めに確保します。").font(.callout).foregroundStyle(.secondary).fixedSize(horizontal:false,vertical:true)
                Divider();Label("投手は1人 · 球種は2種類",systemImage:"person.fill").font(.callout)
            }
        }
    }
    var hitSettings: some View {
        HStack(alignment:.top,spacing:20){
            Card(title:"打撃と得点",icon:"baseball") {
                row("最低得点","静止したバットでもボールは打ち返せます。",\.minimumPoints,0...100,"点")
                row("最高得点","速く振るほど、この点数に近づきます。",\.maximumPoints,max(0,model.settings.minimumPoints)...500,"点")
                row("最高点になる速度","当たったバットの部位の速度で計算します。",\.maximumScoreSpeed,0.5...30,"m/s")
                row("金色の帯の速度加算","実測速度に加えてから得点を計算します。",\.bandSpeedBonus,0...10,"m/s")
                row("マウスの最大ため時間","左ボタンを押し、離すとスイングします。",\.chargeSeconds,0.2...3,"秒")
            }
            VStack(spacing:20){Card(title:"爆弾のペナルティ",icon:"exclamationmark.triangle") {
                row("爆弾の失点","爆弾をバットに当てたときに減点します。",\.bombPenalty,0...1000,"点")
                row("バットの使用不能時間","バットが点滅し、打てない間も投球は続きます。",\.stunSeconds,0...10,"秒")
                Text("野球ボールの空振りは0点。爆弾を避ければ失点しません。合計点は負になる場合があります。").font(.caption).foregroundStyle(.secondary).fixedSize(horizontal:false,vertical:true)
            };Card(title:"得点の目安",icon:"speedometer") {ForEach([0.0,2.0,5.0,8.0],id:\.self){v in HStack{Text("\(v,specifier:"%.1f") m/s");Spacer();Text("通常 \(model.settings.points(v,false)) 点");Text("帯 \(model.settings.points(v,true)) 点").foregroundStyle(.teal)}}}}
        }
    }
    var spaceSettings: some View {
        HStack(alignment:.top,spacing:20){
            VStack(spacing:20){Card(title:"打撃範囲",icon:"viewfinder") {
                row("範囲の幅","ボールの中心が到達する横幅です。",\.zoneWidth,0.3...1.8,"m")
                row("範囲の高さ","中心の高さを基準に上下へ広がります。",\.zoneHeight,0.2...1.1,"m")
                row("範囲の中心の高さ","床からの高さ。VRでは身長補正が加わります。",\.zoneCenterY,0.8...1.7,"m")
            };Card(title:"山なりの軌道",icon:"arrow.up.forward") {
                row("到達までの時間","発射から打撃面への到達まで。手元で減速します。",\.flightSeconds,1...6,"秒")
                row("山なりの高さ","直線に対して軌道の中央を持ち上げます。",\.arcHeight,0.05...2,"m")
                row("発射位置の距離","打撃面はプレイヤー前方0.70 mです。",\.launchDistance,4...16,"m")
            }}
            Card(title:"重なりの回避",icon:"arrow.triangle.branch") {
                row("通常球の到達間隔","到達時刻の最低間隔です。",\.ballGap,0.15...2,"秒")
                row("爆弾を含む到達間隔","爆弾の前後は広めに確保します。",\.bombGap,max(0.15,model.settings.ballGap)...3,"秒")
                row("通常球の軌道間隔","軌道全体で球の中心同士の距離を確保します。",\.clearance,0.2...1,"m")
                row("爆弾を含む軌道間隔","爆弾が近いときはより離します。",\.bombClearance,max(0.2,model.settings.clearance)...1.5,"m")
                Text("到達位置を選び直し、避けられなければ発射を少し遅らせます。そのため、実際のペースは設定より遅くなる場合があります。").font(.caption).foregroundStyle(.secondary).fixedSize(horizontal:false,vertical:true)
            }
        }
    }
    var preview: some View {
        VStack(spacing:18){
            HStack{stat("全投球","\(model.pitches.count)");stat("野球ボール","\(model.pitches.filter{!$0.bomb}.count)");stat("爆弾","\(model.pitches.filter{$0.bomb}.count)");stat("この時点の間隔",String(format:"%.2f 秒",model.settings.interval(model.time)));stat("この時点の爆弾率",String(format:"%.1f %%",model.settings.bombRate(model.time)))}
            PreviewCanvas(settings:model.settings,pitches:model.pitches,time:model.time).frame(height:220).background(Color(red:0.04,green:0.10,blue:0.16),in:RoundedRectangle(cornerRadius:14))
            Card(title:"時間バー",icon:"play.rectangle") {
                HStack{Button {if model.time>=model.settings.roundSeconds+model.settings.flightSeconds {model.time=0};model.playing.toggle()} label:{Label(model.playing ? "一時停止":"再生",systemImage:model.playing ? "pause.fill":"play.fill")}.buttonStyle(.borderedProminent).tint(.teal);Button("最初へ"){model.time=0;model.playing=false};Spacer();Text("\(model.time,specifier:"%.1f") / \(model.settings.roundSeconds,specifier:"%.0f") 秒").monospacedDigit();Button("乱数を変える"){model.settings.seed &+= 1}}
                Slider(value:$model.time,in:0...(model.settings.roundSeconds+model.settings.flightSeconds))
                Canvas{ctx,size in for p in model.pitches {let x=p.launch/(model.settings.roundSeconds+model.settings.flightSeconds)*size.width;ctx.fill(Path(ellipseIn:CGRect(x:x-2,y:1,width:4,height:8)),with:.color(p.bomb ? .orange:.teal))}}.frame(height:12)
                HStack{Text("0秒");Spacer();Text("発射の位置：青＝野球ボール / 橙＝爆弾");Spacer();Text("終了＋残りの球")}.font(.caption).foregroundStyle(.secondary)
            }
            Text("ゲームと同じ軌道計画を使用した乱数例です。打たずに見送った場合を表示します。薄い円は到達予定位置を示します。").font(.caption).foregroundStyle(.secondary)
        }
    }
    func stat(_ title:String,_ value:String)->some View {VStack(alignment:.leading,spacing:7){Text(title).font(.caption).foregroundStyle(.secondary);Text(value).font(.title2.weight(.semibold)).monospacedDigit()}.padding(15).frame(maxWidth:.infinity,alignment:.leading).background(Color(nsColor:.controlBackgroundColor),in:RoundedRectangle(cornerRadius:10))}
}
struct PreviewCanvas: View {
    var settings: Settings,pitches:[Pitch],time:Double
    var body: some View {Canvas {ctx,size in
        let front=CGRect(x:28,y:48,width:size.width*0.43,height:size.height-80),side=CGRect(x:size.width*0.52,y:48,width:size.width*0.45-24,height:size.height-80)
        func label(_ s:String,_ p:CGPoint){ctx.draw(Text(s).font(.caption).foregroundStyle(.white.opacity(0.8)),at:p,anchor:.topLeading)}
        label("到達範囲 · 正面",CGPoint(x:28,y:18));label("山なりの軌道 · 側面",CGPoint(x:side.minX,y:18))
        let minY=settings.zoneCenterY-0.9,maxY=settings.zoneCenterY+1.3
        func xy(_ p:V3)->CGPoint {CGPoint(x:front.midX+p.x/2.4*front.width,y:front.maxY-(p.y-minY)/(maxY-minY)*front.height)}
        func zy(_ p:V3)->CGPoint {CGPoint(x:side.minX+(1-(p.z-0.7)/(settings.launchDistance-0.7))*side.width,y:side.maxY-p.y/(settings.zoneCenterY+settings.arcHeight+1.8)*side.height)}
        let tl=xy(V3(x:-settings.zoneWidth/2,y:settings.zoneCenterY+settings.zoneHeight/2,z:0.7)),br=xy(V3(x:settings.zoneWidth/2,y:settings.zoneCenterY-settings.zoneHeight/2,z:0.7))
        let zone=CGRect(x:tl.x,y:tl.y,width:br.x-tl.x,height:br.y-tl.y)
        ctx.fill(Path(roundedRect:zone,cornerRadius:8),with:.color(.teal.opacity(0.12)));ctx.stroke(Path(roundedRect:zone,cornerRadius:8),with:.color(.teal.opacity(0.7)),lineWidth:1.5)
        var baseline=Path();baseline.move(to:CGPoint(x:side.minX,y:side.maxY));baseline.addLine(to:CGPoint(x:side.maxX,y:side.maxY));ctx.stroke(baseline,with:.color(.white.opacity(0.2)),lineWidth:1)
        label("投手",CGPoint(x:side.minX,y:side.maxY+9));label("打撃面",CGPoint(x:side.maxX-44,y:side.maxY+9))
        for p in pitches where time>=p.launch && time<=p.arrival+0.12 {
            let color:Color=p.bomb ? .orange:.cyan,pos=p.at(time),end=xy(p.end)
            ctx.stroke(Path(ellipseIn:CGRect(x:end.x-8,y:end.y-8,width:16,height:16)),with:.color(color.opacity(0.3)),lineWidth:1)
            var route=Path();for i in 0...50 {let q=zy(p.at(p.launch+p.seconds*Double(i)/50));if i==0{route.move(to:q)}else{route.addLine(to:q)}}
            ctx.stroke(route,with:.color(color.opacity(0.15)),lineWidth:1)
            for pt in [xy(pos),zy(pos)] {let r=p.bomb ? 7.0:5.0;ctx.fill(Path(ellipseIn:CGRect(x:pt.x-r,y:pt.y-r,width:r*2,height:r*2)),with:.color(color));if p.bomb{ctx.stroke(Path(ellipseIn:CGRect(x:pt.x-r-3,y:pt.y-r-3,width:r*2+6,height:r*2+6)),with:.color(color.opacity(0.5)),lineWidth:1)}}
        }
    }}
}
func verifyTuner() {
    let c=Settings(),p=Generator.build(c)
    precondition(!p.isEmpty)
    for i in p.indices {precondition(Generator.safe(p[i],Array(p.prefix(i)),c))}
    var dense=c;dense.intervalStart=0.4;dense.intervalEnd=0.4;dense.bombPercentStart=100;dense.bombPercentEnd=100
    let d=Generator.build(dense);for i in d.indices {precondition(d[i].bomb && Generator.safe(d[i],Array(d.prefix(i)),dense))}
    precondition(c.points(0,false)<c.points(4,false) && c.points(4,false)<c.points(8,false))
    let rows=p.map {["id":Double($0.id),"launch":$0.launch,"x":$0.end.x,"y":$0.end.y,"bomb":$0.bomb ? 1.0:0.0]}
    try! JSONSerialization.data(withJSONObject:rows,options:[.prettyPrinted,.sortedKeys]).write(to:URL(fileURLWithPath:"/tmp/vrbat-tuner-plan.json"))
    let e=JSONEncoder();e.outputFormatting=[.prettyPrinted,.sortedKeys];try! e.encode(c).write(to:URL(fileURLWithPath:"/tmp/vrbat-tuner-default.json"))
    print("TUNER_VERIFY_OK default=\(p.count) denseBombs=\(d.count)")
}
@main struct TunerApp: App {
    init(){if CommandLine.arguments.contains("--verify"){verifyTuner();exit(0)}}
    var body: some Scene {WindowGroup("バット・カーニバル 調整") {ContentView().onAppear {NSApp.setActivationPolicy(.regular);NSApp.activate(ignoringOtherApps:true)}}.defaultSize(width:1080,height:800).commands{CommandGroup(replacing:.newItem){}}}
}
