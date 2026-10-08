"""Recognisable fixed retail fixtures, cinematic landmarks and loading dock."""
import math
from meridian_core import box, mesh, rod, cylinder, ring, export

def fountain(lod=0):
    sides=40 if lod==0 else 24 if lod==1 else 12
    ring('TerrazzoRim',(0,.38,0),2.30,2.00,.62,'Terrazzo',sides)
    ring('BrassBasinLip',(0,.72,0),2.32,1.98,.055,'StorefrontTrim',sides)
    cylinder('BasinFloor',(0,.055,0),2.05,.10,'Tile',sides)
    cylinder('CentralPedestal',(0,.68,0),.33,1.30,'FacadeStone',sides)
    ring('UpperBowl',(0,1.36,0),1.0,.85,.18,'Terrazzo',sides)
    cylinder('BrokenSpout',(0,1.53,0),.055,.34,'Rust',12)
    cylinder('StagnantPool',(0,.118,0),1.96,.004,'Water',sides)
    if lod<2:
        for i in range(8):
            a=i*math.tau/8
            rod('DryNozzle',(math.cos(a)*1.62,.13,math.sin(a)*1.62),
                (math.cos(a)*1.63,.31,math.sin(a)*1.63),.019,'Rust',8)

def planter(lod=0):
    sides=24 if lod==0 else 16 if lod==1 else 8
    ring('StonePlanter',(0,.38,0),.82,.67,.72,'FacadeStone',sides)
    cylinder('Soil',(0,.68,0),.66,.035,'Moss',sides)
    ring('BrassRim',(0,.77,0),.83,.66,.06,'StorefrontTrim',sides)
    if lod<2:
        for i in range(8):
            a=i*2.4; h=.35+(i%3)*.21
            rod('DeadStem',(math.cos(a)*.4,.69,math.sin(a)*.4),
                (math.cos(a)*.4+.06,.69+h,math.sin(a)*.4),.014,'BoardWood',6)

def directory(lod=0):
    box('StoneFoot',(0,.08,0),(1.2,.16,.65),'FacadeStone',.06)
    box('DirectoryCase',(0,1.36,0),(1.35,2.35,.18),'StorefrontTrim',.045)
    for side in (-1,1):
        box('MapBacking',(0,1.42,side*.103),(1.18,1.85,.022),'Plaster',.005)
        if lod==0:
            for i in range(3):
                box('FloorDiagram',(0,.78+i*.51,side*.117),(.91,.38,.009),'Tile',0)
                box('AtriumCutout',(.1,.78+i*.51,side*.124),(.23,.20,.006),'Crack',0)

def kiosk(lod=0):
    box('AttachedKiosk',(0,.55,0),(2.8,1.1,2),'StorefrontTrim',.035)
    box('CounterTop',(0,1.135,0),(3,.10,2.18),'Wood',.03)
    for x in (-1.32,1.32):box('PermanentCanopyPost',(x,1.98,.84),(.07,1.70,.07),'FixtureTrim',.01)
    box('Canopy',(0,2.86,.06),(3.2,.23,2.22),'Plaster',.035)
    if lod<2:
        for i in range(8):box('FasciaSlat',(-1.12+i*.32,.58,1.014),(.12,.97,.025),'Wood',.004)

def food_counter(lod=0):
    box('FixedCounter',(0,.51,0),(3.2,1.02,.90),'Tile',.02)
    box('StainlessWorktop',(0,1.07,0),(3.32,.12,1.04),'EscalatorSteel',.028)
    for x in (-1.5,1.5):rod('SneezeGuardPost',(x,1.13,.41),(x,1.74,.41),.025,'FixtureTrim',8)
    box('DirtySneezeGuard',(0,1.66,.41),(3.10,.23,.022),'Glass',0)
    if lod<2:
        for x in (-1.05,0,1.05):
            box('TrayWell',(x,1.136,-.10),(.88,.025,.57),'FixtureTrim',.03)
            box('EmptyServingTray',(x,1.154,-.10),(.78,.020,.45),'EscalatorSteel',.025)

def table(lod=0):
    box('FoodCourtTable',(0,.77,0),(2.1,.11,1.0),'Plaster',.065)
    for x in (-.78,.78):box('BoltedTrestle',(x,.37,0),(.13,.74,.79),'FixtureTrim',.025)
    for z in (-.78,.78):
        box('FixedBench',(0,.45,z),(2.08,.11,.36),'Wood',.028)
        for x in (-.78,.78):box('BenchPedestal',(x,.2,z),(.12,.4,.31),'FixtureTrim',.015)

def cinema_seat(lod=0):
    box('SeatCushion',(0,.47,0),(.58,.17,.59),'Upholstery',.065,detail=3-lod)
    box('SeatBack',(0,.91,-.23),(.60,.78,.17),'Upholstery',.065,detail=3-lod)
    for x in (-.36,.36):
        box('Armrest',(x,.65,0),(.11,.10,.66),'FixtureTrim',.035)
        box('BoltedFrame',(x,.28,0),(.065,.56,.35),'FixtureTrim',.012)
        if lod==0:ring('Cupholder',(x,.717,.19),.046,.033,.023,'FixtureTrim',12)

def marquee(lod=0):
    box('MarqueeShell',(0,.55,0),(4.0,1.1,.5),'StorefrontTrim',.06)
    box('LetterPanel',(0,.54,.266),(3.72,.86,.025),'Plaster',.018)
    for side in (-1,1):box('BrassBorder',(0,.54+side*.47,.282),(3.92,.035,.035),'StorefrontTrim',.006)
    if lod<2:
        for i in range(20):
            cylinder('SpentBulb',(-1.83+i*.193,.03,.28),.020,.035,'FixtureGlow',8)

def cinema_doors(lod=0):
    for x in (-.875,.875):
        box('TheatreLeaf',(x,1.31,0),(1.69,2.62,.10),'Upholstery',.02)
        box('KickPlate',(x,.34,.057),(1.59,.53,.025),'EscalatorSteel',.007)
        if lod<2:rod('PushBar',(x-.55,1.15,.12),(x+.55,1.15,.12),.027,'EscalatorSteel',12)
    for x in (-1.78,1.78):box('DoorJamb',(x,1.34,0),(.065,2.68,.22),'StorefrontTrim',.006)
    box('Lintel',(0,2.67,0),(3.62,.075,.22),'StorefrontTrim',.008)

def shelf(lod=0):
    for x in (-.9,.9):box('Upright',(x,1.0,0),(.058,2.0,.5),'FixtureTrim',.006)
    box('FixedBacking',(0,1.0,-.255),(1.86,2.0,.04),'Plaster',.006)
    for y in (.1,.57,1.04,1.51,1.97):box('EmptyShelf',(0,y,0),(1.86,.04,.55),'EscalatorSteel',.008)
    if lod==0:
        for y in (.14,.61,1.08,1.55):box('PriceRail',(0,y,.29),(1.80,.037,.025),'StorefrontTrim',.005)

def display(lod=0):
    box('BoltedCabinet',(0,.40,0),(1.9,.8,.65),'Wood',.025)
    box('EmptyVelvetTray',(0,.83,0),(1.84,.06,.60),'Upholstery',.015)
    for x in (-.95,.95):box('DisplayUpright',(x,1.05,0),(.04,.5,.67),'StorefrontTrim',.008)
    box('BrokenBackGlass',(0,1.10,-.32),(1.82,.42,.012),'Glass',0)
    box('IntactGlassTop',(-.45,1.3,0),(.93,.012,.64),'Glass',0)

def security_desk(lod=0):
    box('BuiltInDesktop',(0,.76,0),(2.3,.12,.95),'Wood',.025)
    for x in (-.93,.93):box('Cabinet',(x,.36,0),(.40,.72,.83),'StorefrontTrim',.02)
    box('EmptyMonitorHood',(-.4,1.08,-.13),(.61,.50,.52),'FixtureTrim',.065)
    box('DarkScreen',(-.4,1.08,.144),(.49,.35,.014),'Glass',0)
    if lod<2:
        for y in (.2,.42,.64):box('DrawerLine',(.93,y,.424),(.35,.015,.01),'Crack',0)

def mannequin(lod=0):
    sides=16 if lod==0 else 10 if lod==1 else 6
    cylinder('Stand',(0,.035,0),.28,.07,'FixtureTrim',sides)
    rod('Support',(0,.04,0),(0,.91,0),.027,'FixtureTrim',sides)
    box('Torso',(0,1.29,0),(.37,.48,.21),'Plaster',.1,detail=3-lod)
    box('Pelvis',(0,.96,0),(.28,.20,.19),'Plaster',.07,detail=3-lod)
    box('BlankHead',(0,1.72,0),(.20,.28,.17),'Plaster',.065,detail=3-lod)
    for sign in (-1,1):
        rod('Leg',(sign*.09,.15,0),(sign*.10,.94,0),.065,'Plaster',sides)
        rod('Arm',(sign*.22,1.42,0),(sign*.29,.83,.055),.044,'Plaster',sides)

def dock(lod=0):
    box('DockPitFrame',(0,.045,0),(3.6,.09,2.5),'FixtureTrim',.025)
    box('SteelLeveller',(0,.12,0),(3.30,.16,2.2),'BridgeMetal',.025)
    box('HingedDockLip',(0,.06,1.32),(3.30,.075,.48),'EscalatorSteel',.016)
    for x in (-1.88,1.88):box('RubberDockBumper',(x,.28,1.04),(.24,.55,.42),'FixtureTrim',.06)
    if lod==0:
        for x in (-1.53,1.53):
            for z in (-.9,.9):cylinder('Bolt',(x,.21,z),.022,.019,'EscalatorSteel',8)

def fixture(lod=0):
    box('LuminaireHousing',(0,-.04,0),(1.30,.08,.28),'FixtureTrim',.018)
    for x in (-.61,.61):box('EndCap',(x,-.078,0),(.085,.085,.26),'StorefrontTrim',.012)
    if lod==0:
        diffuser=box('Diffuser',(0,-.09,0),(1.12,.04,.20),'FixtureGlow',.012)
        diffuser['SeparateRenderer']=True

def service_pipe(lod=0, conduit=False):
    radius=.018 if conduit else .065
    rod('Conduit' if conduit else 'WaterPipe',(-2,.12,0),(2,.12,0),radius,'FixtureTrim' if conduit else 'Rust',12 if lod==0 else 8)
    for x in (-1.5,0,1.5):
        box('WallBracket',(x,.04,0),(.045,.12,.18),'FixtureTrim',.006)
        if not conduit and lod==0:ring('PipeUnion',(x,.12,0),.076,.058,.055,'Rust',12)

def generate():
    models={'Fountain':fountain,'Planter':planter,'MallDirectory':directory,'Directory':directory,
        'Kiosk':kiosk,'FoodCounter':food_counter,'FoodCourtCounter':food_counter,'FoodCourtTable':table,
        'CinemaSeat':cinema_seat,'CinemaSeats':cinema_seat,'CinemaMarquee':marquee,'CinemaDoors':cinema_doors,
        'RetailShelf':shelf,'EmptyDisplayCase':display,'SecurityDesk':security_desk,'Mannequin':mannequin,
        'DockLeveller':dock,'FluorescentFixture':fixture,'WallConduit':lambda l:service_pipe(l,True),
        'WallPipe':service_pipe,'Vent':lambda l:box('VentDuct',(0,.20,0),(4,.40,.55),'EscalatorSteel',.035)}
    for name,maker in models.items():export(name,maker)
