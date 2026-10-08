"""Manufactured mall shell, storefronts, flights and ceiling modules."""
import math
from meridian_core import box, mesh, rod, cylinder, export

def wall(block=False, lod=0):
    key='Block' if block else 'Plaster'
    box('WallCore',(0,1.875,0),(4,3.75,.24),key,.018,detail=2-lod//2)
    if not block:
        for side in (-1,1):
            box('TileSkirting',(0,.13,side*.134),(4,.26,.035),'Tile',.006)
            box('DadoCap',(0,1.02,side*.136),(4,.035,.036),'StorefrontTrim',.006)

def storefront(entry=False, boarded=False, lod=0):
    for sign in (-1,1):
        box('StonePier',(sign*1.87,1.74,0),(.26,3.48,.42),'FacadeStone',.02)
        box('PowderCoatedJamb',(sign*1.72,1.65,.07),(.07,3.3,.18),'StorefrontTrim',.01)
    box('Bulkhead',(0,3.5,0),(4,.5,.52),'StorefrontTrim',.025)
    box('BrassReveal',(0,3.23,.282),(3.46,.04,.035),'StorefrontTrim',.004)
    if not entry:
        box('DisplayPlinth',(0,.15,0),(3.4,.3,.40),'FacadeStone',.012)
        if boarded:
            for i in range(7):
                box('WeatheredBoard',(0,.52+i*.38,.10),(3.35,.35,.042),'BoardWood',.006,rot=(-3 if i%2 else 2))
        else:
            # Glazing gets a distinct Water slot for Unity's transparent glass remap.
            box('DisplayGlass',(0,1.8,.092),(3.32,2.99,.018),'Glass',0)
            if lod<2:
                for x in (-.55,.55): box('GlazingMullion',(x,1.8,.10),(.045,3,.07),'StorefrontTrim',.005)

def column(lod=0):
    box('ConcreteColumn',(0,1.875,0),(.58,3.75,.58),'Plaster',.045,detail=max(1,3-lod))
    box('StonePlinth',(0,.16,0),(.7,.32,.7),'FacadeStone',.025)
    box('Crown',(0,3.64,0),(.7,.22,.7),'StorefrontTrim',.02)
    if lod<2:
        for side in (-1,1):box('VerticalFlute',(side*.296,1.89,0),(.012,3.2,.15),'StorefrontTrim',.002)

def railing(lod=0):
    sides=12 if lod==0 else 8
    for x in (-1.96,0,1.96):
        box('Shoe',(x,.035,0),(.14,.07,.18),'EscalatorSteel',.014)
        box('Post',(x,.56,0),(.052,1.09,.065),'EscalatorSteel',.008)
    rod('Handrail',(-2,1.12,0),(2,1.12,0),.038,'EscalatorSteel',sides)
    box('LaminatedGlass',(0,.59,0),(3.85,.91,.018),'Glass',0)
    box('BaseChannel',(0,.12,0),(4,.055,.09),'EscalatorSteel',.007)

def flight(kind, lod=0):
    haul=kind=='HaulRamp'; esc=kind=='Escalator'; rise=2 if haul else 4
    width=4 if haul else 2.4 if esc else 3; metal='EscalatorSteel' if esc or haul else 'Concrete'
    # Solid load-bearing envelope hides the matching gameplay ramp without geometry over void.
    verts=[(-width/2,-.16,0),(width/2,-.16,0),(width/2,rise-.16,8),(-width/2,rise-.16,8),
           (-width/2,.04 if haul else .1,0),(width/2,.04 if haul else .1,0),
           (width/2,rise+.04 if haul else rise+.1,8),(-width/2,rise+.04 if haul else rise+.1,8)]
    mesh('ContinuousSupport',verts,[(0,1,2,3),(4,7,6,5),(0,4,5,1),(1,5,6,2),(2,6,7,3),(3,7,4,0)],metal)
    if not haul:
        for i in range(20):
            top=(i+1)*.2
            box('Tread',(0,top-.06,(i+.5)*.4),(width,.12,.4),metal,.009,detail=1)
            if lod<2:
                box('Nosing',(0,top+.002,i*.4+.025),(width,.012,.035),'StorefrontTrim',.002)
                if esc and lod==0:
                    for j in range(12):box('StepGroove',(-width*.5+.08+j*(width-.16)/11,top+.007,(i+.5)*.4),(.009,.010,.34),'Crack',0)
    else:
        if lod<2:
            for i in range(24):
                z=(i+.5)*8/24
                box('TractionStrip',(0,z*rise/8+.045,z),(width,.008,.06),'FixtureTrim',0)
    for sign in (-1,1):
        x=sign*(width/2+.15)
        rod('ContinuousHandrail',(x,1.03,0),(x,rise+1.03,8),.055,'FixtureTrim',12 if lod==0 else 8)
        if esc:
            mesh('StainlessSide',[(x,.05,0),(x,rise+.05,8),(x,rise+.83,8),(x,.83,0)],[(0,1,2,3)],'EscalatorSteel')
            box('Newel',(x,.52,.05),(.24,1.0,.5),'EscalatorSteel',.085,detail=3-lod)
        else:
            for i in range(5 if lod<2 else 3):
                z=i*8/(4 if lod<2 else 2)
                rod('Baluster',(x,z*rise/8,z),(x,z*rise/8+1.0,z),.027,'FixtureTrim',8)

def ceiling(kind, lod=0):
    for i in range(5):
        box('MainTbar',(-2+i,0,0),(.025,.06,4),'FixtureTrim',.003)
        box('CrossTbar',(0,0,-2+i),(4,.06,.025),'FixtureTrim',.003)
    for x in range(4):
        for z in range(4):
            if kind=='Damaged' and (x,z) in ((0,0),(0,1),(2,2)):continue
            y=-.018-(.17 if kind=='Damaged' and x==2 else 0)
            tile=box('MineralPanel',(-1.5+x,y,-1.5+z),(.965,.033,.965),'Ceiling',.003)
            if kind=='Damaged' and x==2:tile.rotation_euler.x=.1
            if kind=='Coffer' and lod<2:
                for sign in (-1,1):
                    box('CofferLip',(-1.5+x+sign*.4,-.11,-1.5+z),(.08,.18,.90),'Plaster',.01)
    if kind=='Damaged' and lod<2:
        for i in range(3):rod('LooseCable',(.12+i*.17,-.01,.3),(.15+i*.18,-.62,.4),.008,'FixtureTrim',6)

def skylight(lod=0):
    for x in (-8,8):box('PerimeterRib',(x,.30,0),(.24,.6,16),'StorefrontTrim',.035)
    for z in (-8,8):box('PerimeterRib',(0,.30,z),(16,.6,.24),'StorefrontTrim',.035)
    for i in range(7):
        x=-6+i*2
        rod('RoofRafter',(x,0,-8),(x,1.25,0),.09,'EscalatorSteel',8)
        rod('RoofRafter',(x,1.25,0),(x,0,8),.09,'EscalatorSteel',8)
    for z in (-4,0,4):box('Purlin',(0,1.25-abs(z)*.15625,z),(16,.12,.12),'EscalatorSteel',.008)
    if lod<2:
        for x in (-7,-5,-3,-1,1,3,5,7):
            for z in (-6,-2,2,6):
                if (x,z) in ((-1,2),(1,2)):continue
                y=1.25-abs(z)*.15625
                panel=box('SkylightGlass',(x,y,z),(1.85,.025,3.82),'Glass',0)
                panel.rotation_euler.x=math.atan(.15625)*(1 if z<0 else -1)

def canopy(lod=0):
    box('EntranceCrown',(0,3.7,0),(12,.38,4),'FacadeStone',.055)
    box('Soffit',(0,3.45,0),(11.8,.12,3.8),'Ceiling',.015)
    for x in (-5.5,5.5):
        cylinder('CanopyColumn',(x,1.65,-.9),.18,3.3,'FacadeStone',24 if lod==0 else 12)
    for x in range(-5,6):box('VerticalFascia',(x,3.75,2.025),(.035,.4,.03),'StorefrontTrim',0)

def fins(lod=0):
    box('AnchorPier',(0,1.875,0),(4,3.75,.32),'FacadeStone',.035)
    for x in (-1.6,-.8,0,.8,1.6):box('StoneFin',(x,1.875,.26),(.15,3.75,.48),'FacadeStone',.025)

def shutter(lod=0):
    box('ShutterBacking',(0,1.55,0),(3.4,3.1,.075),'BridgeMetal',.006)
    if lod<2:
        for i in range(26):box('RolledSlat',(0,.065+i*.118,.055),(3.4,.026,.018),'EscalatorSteel',.006,detail=1)
    box('BottomLockRail',(0,.04,.035),(3.4,.08,.14),'FixtureTrim',.013)
    box('LockPlate',(0,.20,.09),(.13,.2,.025),'Rust',.009)

def generate():
    for name,maker in {'WallPlaster':lambda l:wall(False,l),'WallBlock':lambda l:wall(True,l),
        'StorefrontGlazed':lambda l:storefront(False,False,l),'StorefrontEntry':lambda l:storefront(True,False,l),
        'StorefrontBoarded':lambda l:storefront(False,True,l),'Column':column,'GlassRail4m':railing,
        'Escalator':lambda l:flight('Escalator',l),'ServiceStair':lambda l:flight('ServiceStair',l),
        'HaulRamp':lambda l:flight('HaulRamp',l),'CeilingGrid':lambda l:ceiling('Grid',l),
        'CeilingCoffer':lambda l:ceiling('Coffer',l),'CeilingDamaged':lambda l:ceiling('Damaged',l),
        'CeilingSagging':lambda l:ceiling('Damaged',l),'SkylightFrame':skylight,
        'EntranceCanopy':canopy,'FacadeFins':fins,'RollerShutter':shutter}.items():export(name,maker)
