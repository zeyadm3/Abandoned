"""Original fixed mall dressing for 0.13.2; no new textures or external assets."""
import math
from meridian_core import box, rod, cylinder, ring, export

def bolts(points, lod=0):
    if lod: return
    for p in points: cylinder('AnchorBolt',p,.018,.012,'EscalatorSteel',8)

def bench(lod=0):
    for x in (-1.12,1.12):
        box('BoltedPedestal',(x,.20,0),(.11,.40,.53),'FixtureTrim',.018)
        box('AnchorFoot',(x,.035,0),(.34,.07,.66),'FixtureTrim',.020)
        rod('BackSupport',(x,.37,-.23),(x,.93,-.27),.035,'EscalatorSteel',12 if lod==0 else 6)
        bolts([(x+side*.10,.078,z) for side in (-1,1) for z in (-.22,.22)],lod)
    for z in (-.25,0,.25):box('SeatSlat',(0,.44,z),(3.0,.08,.18),'Wood',.025)
    for y in (.65,.87):box('BackSlat',(0,y,-.29),(3.0,.14,.075),'Wood',.020)
    for x in (-1.38,1.38):
        rod('ArmPost',(x,.42,-.25),(x,.70,-.25),.035,'FixtureTrim',8)
        rod('ArmRest',(x,.70,-.26),(x,.70,.25),.045,'EscalatorSteel',12 if lod==0 else 6)
    if lod<2:rod('TieBar',(-1.18,.25,0),(1.18,.25,0),.025,'FixtureTrim',8)

def mall_bin(lod=0):
    sides=(32,20,12)[lod]
    cylinder('BoltedBinFoot',(0,.035,0),.33,.07,'FixtureTrim',sides)
    ring('BinBody',(0,.52,0),.34,.29,.95,'StorefrontTrim',sides)
    cylinder('DarkInterior',(0,.14,0),.285,.025,'Carpet',sides)
    ring('Rim',(0,1.0,0),.36,.27,.08,'EscalatorSteel',sides)
    for x in (-.27,.27):box('LidSupport',(x,1.08,0),(.045,.18,.045),'FixtureTrim',.008)
    box('RainHood',(0,1.185,0),(.72,.045,.72),'EscalatorSteel',.04)
    if lod<2:
        for i in range(12 if lod==0 else 6):
            a=i*math.tau/(12 if lod==0 else 6)
            rod('VerticalRib',(math.sin(a)*.343,.12,math.cos(a)*.343),
                (math.sin(a)*.343,.91,math.cos(a)*.343),.009,'FixtureTrim',6)

def lightbox(lod=0):
    box('FixedPlinth',(0,.035,0),(.92,.07,.32),'FacadeStone',.035)
    for x in (-.52,.52):box('MountingLeg',(x,.25,0),(.07,.50,.15),'FixtureTrim',.012)
    box('DisplayCase',(0,1.35,0),(1.35,1.90,.27),'StorefrontTrim',.035)
    for side in (-1,1):
        box('WornBlankPoster',(0,1.35,side*.145),(1.18,1.73,.018),'Plaster',.008)
        if lod<2:
            for y in (.46,2.24):box('TrimStrip',(0,y,side*.159),(1.29,.025,.018),'EscalatorSteel',.004)
    if lod==0:
        for y in (.55,2.15):box('AccessScrew',( .59,y,.169),(.020,.020,.007),'FixtureTrim',.004)

def rack(lod=0):
    for x in (-1.30,1.30):
        box('BoltedRailFoot',(x,.035,0),(.26,.07,.80),'FixtureTrim',.025)
        rod('RailUpright',(x,.07,0),(x,1.78,0),.037,'EscalatorSteel',12 if lod==0 else 6)
        bolts([(x,.078,z) for z in (-.28,.28)],lod)
    rod('GarmentRail',(-1.35,1.77,0),(1.35,1.77,0),.033,'EscalatorSteel',12 if lod==0 else 6)
    rod('LowerBrace',(-1.30,.28,0),(1.30,.28,0),.023,'FixtureTrim',8)
    if lod<2:
        for i in range(13 if lod==0 else 5):
            x=-1.12+i*(2.24/(12 if lod==0 else 4))
            points=[(x,1.74,0),(x+.045,1.68,0),(x,1.62,0),(x-.23,1.42,0),(x+.23,1.42,0),(x,1.62,0)]
            for a,b in zip(points,points[1:]):rod('EmptyFixedHanger',a,b,.010,'FixtureTrim',6)

def gondola(lod=0):
    box('FixedBase',(0,.08,0),(3.0,.16,1.35),'StorefrontTrim',.025)
    box('DoubleSidedBack',(0,.95,0),(2.90,1.70,.09),'Plaster',.008)
    for x in (-1.44,1.44):box('ShelfUpright',(x,.94,0),(.07,1.72,.18),'FixtureTrim',.008)
    for side in (-1,1):
        for y in (.20,.62,1.05,1.48):
            box('EmptyMerchShelf',(0,y,side*.34),(2.90,.045,.62),'EscalatorSteel',.008)
            if lod<2:box('PriceStrip',(0,y+.035,side*.65),(2.89,.055,.022),'StorefrontTrim',.004)
        if lod==0:
            for x in (-1.2,0,1.2):
                for y in (.30,.74,1.18):box('ShelfBracket',(x,y,side*.18),(.025,.16,.34),'FixtureTrim',.005)

def cabinet(width=3.2, depth=1.1, lod=0):
    box('BuiltInKickboard',(0,.07,0),(width-.14,.14,depth-.10),'FixtureTrim',.012)
    box('FixedCabinet',(0,.53,0),(width, .92,depth),'Wood',.025)
    box('CounterSlab',(0,1.05,0),(width+.08,.12,depth+.06),'Plaster',.035)
    for x in (-width*.32,0,width*.32):
        for y in (.32,.68):
            box('DrawerFront',(x,y,depth*.5+.012),(width*.30,.31,.025),'StorefrontTrim',.008)
            if lod<2:rod('DrawerPull',(x-.17,y+.07,depth*.5+.05),(x+.17,y+.07,depth*.5+.05),.012,'EscalatorSteel',8)

def checkout(lod=0):
    cabinet(lod=lod)
    box('AttachedBaggingShelf',(-1.05,.75,-.10),(.91,.055,.75),'EscalatorSteel',.015)
    box('BuiltInTillStand',(.90,1.145,-.19),(.49,.07,.40),'FixtureTrim',.015)
    if lod<2:box('CounterCableGrommet',(.7,1.118,-.35),(.08,.012,.07),'Carpet',.018)

def booth(lod=0):
    for side in (-1,1):
        box('BoltedSeatBase',(0,.23,side*.82),(2.70,.46,.52),'StorefrontTrim',.035)
        box('FixedSeat',(0,.51,side*.80),(2.80,.14,.63),'Upholstery',.055,detail=3-lod)
        box('FixedBackrest',(0,.94,side*1.0),(2.80,.82,.18),'Upholstery',.055,detail=3-lod)
        if lod<2:
            for x in (-.93,0,.93):box('UpholsterySeam',(x,.99,side*.90),(.012,.60,.012),'Carpet',0)
    box('BoltedTablePedestal',(0,.38,0),(.24,.76,.36),'FixtureTrim',.025)
    box('TableTop',(0,.80,0),(2.35,.10,.95),'Plaster',.05)

def food_station(lod=0):
    box('TiledCabinet',(0,.49,0),(3.0,.98,.90),'Tile',.020)
    box('Splashback',(0,1.27,-.42),(3.0,.56,.06),'Tile',.010)
    # Frame an actual basin recess instead of a flat black sink decal.
    box('LeftDrainer',(-.94,1.03,0),(1.12,.08,.94),'EscalatorSteel',.015)
    box('RightDrainer',(.94,1.03,0),(1.12,.08,.94),'EscalatorSteel',.015)
    for z in (-.41,.41):box('SinkRim',(0,1.03,z),(.82,.08,.12),'EscalatorSteel',.012)
    box('BasinBottom',(0,.86,0),(.75,.035,.63),'FixtureTrim',.025)
    for x in (-.37,.37):box('BasinWall',(x,.94,0),(.045,.19,.74),'EscalatorSteel',.015)
    rod('TapStem',(.16,1.08,-.32),(.16,1.39,-.32),.023,'EscalatorSteel',12 if lod==0 else 6)
    rod('TapSpout',(.16,1.39,-.32),(.16,1.39,-.09),.025,'EscalatorSteel',12 if lod==0 else 6)
    if lod<2:
        for i in range(8):box('DrainRib',(-1.35+i*.11,1.077,0),(.012,.008,.70),'FixtureTrim',0)

def cinema_screen(lod=0):
    box('FixedScreenBacking',(0,1.60,0),(3.40,3.20,.18),'FixtureTrim',.025)
    box('WornBlankScreen',(0,1.62,.10),(3.13,2.94,.015),'Plaster',.010)
    for x in (-1.63,1.63):box('ScreenMaskSide',(x,1.60,.15),(.14,3.20,.14),'Carpet',.012)
    for y in (.07,3.13):box('ScreenMaskBorder',(0,y,.15),(3.40,.14,.14),'Carpet',.012)
    if lod<2:
        for x in (-1.4,1.4):box('WallMountBracket',(x,1.6,-.14),(.12,2.80,.10),'FixtureTrim',.010)

def ticket_counter(lod=0):
    cabinet(2.94,1.0,lod)
    for x in (-1.42,1.42):box('FixedTicketPost',(x,1.27,-.35),(.09,.42,.13),'StorefrontTrim',.015)
    box('QueueHeader',(0,1.53,-.35),(3.04,.11,.19),'StorefrontTrim',.020)
    box('BoltedTicketWindow',(0,1.30,-.35),(2.74,.36,.025),'Glass',0)
    if lod<2:
        for x in (-.69,.69):box('CounterHatch',(x,1.119,.12),(.50,.015,.36),'EscalatorSteel',.012)

def cart(lod=0):
    for x in (-.48,.48):
        for z in (-.24,.24):
            rod('CastorAxle',(x-.04,.07,z),(x+.04,.07,z),.07,'Carpet',16 if lod==0 else 8)
            box('CastorBracket',(x,.18,z),(.12,.14,.09),'FixtureTrim',.012)
    box('WireBasketFloor',(0,.27,0),(1.20,.04,.75),'EscalatorSteel',.010)
    for x in (-.58,.58):
        for z in (-.34,.34):rod('BasketCorner',(x,.27,z),(x,.95,z),.018,'EscalatorSteel',8)
    for y in (.50,.73,.95):
        for z in (-.35,.35):rod('BasketRail',(-.59,y,z),(.59,y,z),.012,'EscalatorSteel',8)
        for x in (-.59,.59):rod('BasketRail',(x,y,-.35),(x,y,.35),.012,'EscalatorSteel',8)
    if lod<2:
        for i in range(10 if lod==0 else 5):
            x=-.52+i*(1.04/(9 if lod==0 else 4))
            for z in (-.35,.35):rod('BasketWire',(x,.28,z),(x,.95,z),.008,'FixtureTrim',6)
    for x in (-.58,.58):rod('PushHandleLeg',(x,.72,-.35),(x,1.09,-.40),.024,'FixtureTrim',8)
    rod('PushHandle',(-.58,1.09,-.40),(.58,1.09,-.40),.028,'FixtureTrim',8)

def stock_cage(lod=0):
    for x in (-1.45,1.45):
        for z in (-.56,.56):box('RackUpright',(x,1.175,z),(.09,2.35,.09),'FixtureTrim',.010)
    for y in (.13,.85,1.55,2.29):box('EmptyStockShelf',(0,y,0),(2.95,.065,1.20),'EscalatorSteel',.012)
    for y in (.6,1.2,1.8):
        rod('CageRearHorizontal',(-1.42,y,-.59),(1.42,y,-.59),.012,'FixtureTrim',6)
        for x in (-1.49,1.49):rod('CageEndHorizontal',(x,y,-.55),(x,y,.55),.012,'FixtureTrim',6)
    if lod<2:
        for i in range(11 if lod==0 else 5):
            x=-1.35+i*(2.70/(10 if lod==0 else 4))
            rod('CageWire',(x,.15,-.59),(x,2.27,-.59),.008,'FixtureTrim',6)
        for i in range(4):box('FlattenedCardboard',(-.8+i*.5,.185+i*.012,.12),(.71,.012,.76),'BoardWood',0)

def ceiling_duct(lod=0):
    box('RectangularDuct',(0,-.31,0),(4.0,.48,.70),'EscalatorSteel',.030)
    for x in (-1.86,-.62,.62,1.86):box('DuctSeamBand',(x,-.31,0),(.035,.51,.73),'FixtureTrim',.008)
    for x in (-1.4,1.4):
        box('CeilingBracket',(x,-.04,0),(.12,.08,.86),'FixtureTrim',.008)
        if lod==0:bolts([(x,-.082,z) for z in (-.35,.35)])
    box('VentFrame',(0,-.56,0),(1.6,.045,.56),'FixtureTrim',.015)
    if lod<2:
        for i in range(12 if lod==0 else 6):box('VentLouvre',(-.70+i*(1.40/(11 if lod==0 else 5)),-.59,0),(.045,.025,.46),'EscalatorSteel',.004)

def shuttered_kiosk(lod=0):
    box('PermanentKioskBase',(0,.49,0),(3.25,.98,2.35),'StorefrontTrim',.025)
    box('AttachedCounter',(0,1.04,0),(3.40,.12,2.50),'Wood',.030)
    for x in (-1.52,1.52):
        for z in (-1.10,1.10):box('KioskPost',(x,1.90,z),(.11,1.80,.11),'FixtureTrim',.015)
    box('KioskBulkhead',(0,2.72,0),(3.40,.16,2.50),'Plaster',.030)
    for side in (-1,1):
        box('ClosedDisplayGrille',(0,1.89,side*1.09),(2.95,1.54,.045),'EscalatorSteel',.010)
        if lod<2:
            for i in range(13 if lod==0 else 7):box('GrilleSlat',(0,1.19+i*(1.39/(12 if lod==0 else 6)),side*1.121),(2.95,.015,.021),'FixtureTrim',.003)
    box('PadlockPlate',(0,1.26,1.145),(.15,.19,.025),'Rust',.010)

def generate():
    models={'ConcourseBench':bench,'MallBin':mall_bin,'AdvertisingLightbox':lightbox,
        'ClothingRack':rack,'RetailGondola':gondola,'CheckoutCounter':checkout,'FoodBooth':booth,
        'FoodServiceStation':food_station,'CinemaScreen':cinema_screen,'TicketCounter':ticket_counter,
        'ServiceCart':cart,'StockCage':stock_cage,'CeilingDuct':ceiling_duct,'ShutteredKiosk':shuttered_kiosk}
    for name,maker in models.items():export(name,maker)
