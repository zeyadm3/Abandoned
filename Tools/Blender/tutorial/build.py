"""Original 14 Larch Court furniture. Coordinates and pivots match TutorialHouseFittings.
Run: Blender --background --python Tools/Blender/tutorial/build.py -- <repository root>
"""
import os, sys, math
import bpy
from mathutils import Vector
HERE = os.path.dirname(os.path.abspath(__file__))
sys.path[:0] = [HERE, os.path.dirname(HERE)]
import meridian_core as c
from textures import generate
ROOT = sys.argv[sys.argv.index('--')+1]
ONLY = set(sys.argv[sys.argv.index('--')+2:])
OUT = os.path.join(ROOT,'Game/Assets/_Project/Art/Custom/Environment/LarchCourt')
if not ONLY: generate(os.path.join(OUT,'Textures'))
c.initialize(OUT,['Oak','OakDark','Cabinet','SofaFabric','Bedding','Mattress','Pillow','Curtain','Porcelain','Chrome','Brass','DarkMetal','Trim','Cardboard','Tile','Laminate','Rug','TreeGreen','Bark'])
c.UV_METRES.update({'Oak':1,'OakDark':1,'Cabinet':1,'SofaFabric':.6,'Bedding':.8,'Mattress':.6,'Pillow':.6,'Curtain':.6,'Tile':1.2})

def box(name,p,s,mat='Oak',bevel=.02,detail=3): return c.box(name,p,s,mat,bevel=bevel,detail=detail)
def rod(name,a,b,r=.015,mat='Chrome',sides=12): return c.rod(name,a,b,r,mat,sides)
def loop(name,points,r,mat,level=0):
    for a,b in zip(points,points[1:]): rod(name,a,b,r,mat,8 if level else 12)

def pivot(obj, p):
    """The core ring has world-authored vertices: rotate about its own centre, not the file origin."""
    at=Vector(c.xyz(p))
    for vertex in obj.data.vertices: vertex.co-=at
    obj.location=at
    return obj

def pillow(name,p,s,mat,level):
    # Rounded textile silhouette, compressed at its edge and slightly irregular across its face.
    o = box(name,p,s,mat,min(s)*.43,2 if level else 5)
    for poly in o.data.polygons: poly.use_smooth=True
    mod=o.modifiers.new('Weighted upholstery normals','WEIGHTED_NORMAL');mod.keep_sharp=True
    bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=mod.name)
    return o

def sofa(level,chair=False):
    w=.82 if chair else 2.1; d=.82 if chair else .9; n=1 if chair else 3
    for x in [-w/2+.11,w/2-.11]:
        for z in [-d/2+.11,d/2-.11]:box('TaperedFoot',(x,.09,z),(.07,.18,.07),'OakDark',.012)
    pillow('UpholsteredBase',(0,.25,0),(w,.24,d),'SofaFabric',level)
    pillow('BackFrame',(0,.61,-d/2+.12),(w,.53,.20),'SofaFabric',level)
    seat=(w-.3)/n
    for i in range(n):
        x=(i-(n-1)/2)*seat
        pillow('SeatCushion',(x,.43,.06),(seat-.018,.16,d-.23),'SofaFabric',level)
        back=pillow('BackCushion',(x,.67,-d/2+.24),(seat-.025,.41,.15),'SofaFabric',level)
        back.rotation_euler.x=math.radians(-8)
        if level<2:
            for y in [.39,.475]:
                loop('PipedSeam',[(x-seat/2+.03,y,-.17),(x-seat/2+.03,y,d/2-.065),(x+seat/2-.03,y,d/2-.065),(x+seat/2-.03,y,-.17)],.004,'SofaFabric',level)
    for x in [-w/2+.08,w/2-.08]:pillow('RolledArm',(x,.49,0),(.17,.35,d),'SofaFabric',level)
    p=pillow('LooseThrowPillow',(-w*.27,.64,.02),(.35,.32,.13),'Curtain',level);p.rotation_euler.y=.18

def bed(level):
    for x in [-.65,.65]:
        for z in [-.9,.9]:box('Post',(x,.22,z),(.085,.44,.085),'OakDark')
    box('BedFrame',(0,.24,0),(1.5,.22,2),'OakDark')
    pillow('QuiltedMattress',(0,.43,.02),(1.44,.23,1.93),'Mattress',level)
    pillow('Duvet',(0,.565,.27),(1.48,.10,1.42),'Bedding',level)
    pillow('FoldedTop',(0,.605,-.40),(1.48,.10,.24),'Bedding',level)
    for x in [-.36,.36]:pillow('Pillow',(x,.6,-.73),(.61,.14,.38),'Pillow',level)
    box('Headboard',(0,.62,-1.025),(1.56,1.02,.065),'OakDark')
    box('CrownRail',(0,1.14,-1.025),(1.59,.055,.09),'Oak')
    for x in [-.4,0,.4]:box('InsetPanel',(x,.79,-.981),(.35,.49,.018),'Oak',.006)
    box('Footboard',(0,.35,1.02),(1.56,.47,.05),'OakDark')
    if level<2:
        for x in [-.65,-.43,-.21,.01,.23,.45,.67]:
            rod('QuiltStitch',(x,.618,-.26),(x,.618,.91),.002,'Bedding',6)

def toilet(level):
    sides=24 if level else 48
    pillow('Pedestal',(0,.16,.02),(.26,.32,.39),'Porcelain',level)
    # Open oval bowl with rolled lip, recessed dark water and no flat slab across the opening.
    rings=[(.20,.12,.18),(.34,.19,.26),(.41,.195,.27),(.43,.18,.255),(.41,.145,.215),(.29,.10,.15)]
    verts=[(math.cos(i*math.tau/sides)*rx,y, .08+math.sin(i*math.tau/sides)*rz) for y,rx,rz in rings for i in range(sides)]
    faces=[(r*sides+i,r*sides+(i+1)%sides,(r+1)*sides+(i+1)%sides,(r+1)*sides+i) for r in range(len(rings)-1) for i in range(sides)]
    o=c.mesh('OpenBowl',verts,faces,'Porcelain')
    for p in o.data.polygons:p.use_smooth=True
    water=c.cylinder('BowlWater',(0,.29,.08),.11,.008,'DarkMetal',sides);water.scale.y=1.4
    pillow('Cistern',(0,.6,-.20),(.43,.38,.20),'Porcelain',level)
    pillow('CisternLid',(0,.807,-.2),(.45,.035,.22),'Porcelain',level)
    rod('FlushLever',(-.14,.72,-.09),(-.07,.72,-.09),.012,'Chrome')
    for x in [-.14,.14]:box('BoltCap',(x,.045,-.09),(.035,.025,.035),'Porcelain')

def basin(level,ceramic=False):
    # Outer rim at y=0, open well below the countertop surface.
    mat='Porcelain' if ceramic else 'Chrome'
    w=.45 if ceramic else .72; d=.34 if ceramic else .42
    box('RecessedWell',(0,-.14,.02),(w-.08,.018,d-.08),mat,.008)
    for x in [-w/2,w/2]:box('BasinSide',(x,-.065,.02),(.035,.17,d),mat,.012)
    for z in [.02-d/2,.02+d/2]:box('BasinEnd',(0,-.065,z),(w,.17,.035),mat,.012)
    for x in [-w/2-.01,w/2+.01]:box('Rim',(x,.018,.02),(.045,.018,d+.055),mat,.006)
    for z in [.02-d/2-.01,.02+d/2+.01]:box('Rim',(0,.018,z),(w+.055,.018,.045),mat,.006)
    c.cylinder('Drain',(0,-.126,.02),.026,.008,'DarkMetal',20)
    points=[(0,.02,-.24),(0,.20,-.24),(0,.24,-.20),(0,.24,-.08),(0,.20,-.045)]
    loop('GooseneckTap',points,.017,'Chrome',level)
    for x in [-.12,.12]:
        c.cylinder('TapBase',(x,.03,-.23),.026,.026,'Chrome',16)
        rod('TapLever',(x,.07,-.23),(x+.04,.07,-.23),.009,'Chrome')

def tub(level):
    box('Apron',(0,.27,.33),(1.6,.54,.06),'Porcelain')
    box('Well',(0,.13,0),(1.48,.04,.60),'Porcelain')
    for x in [-.76,.76]:box('EndWall',(x,.32,0),(.08,.4,.68),'Porcelain')
    box('BackWall',(0,.32,-.32),(1.6,.4,.06),'Porcelain')
    for z in [-.34,.34]:box('RolledRim',(0,.54,z),(1.6,.045,.075),'Porcelain')
    for x in [-.76,.76]:box('RolledRim',(x,.54,0),(.085,.045,.68),'Porcelain')
    rod('Spout',(-.65,.70,-.32),(-.65,.70,-.17),.025,'Chrome')
    c.cylinder('Drain',(-.6,.155,0),.026,.01,'Chrome',24)
    rod('CurtainRod',(-.8,2,.34),(.8,2,.34),.014,'Chrome')
    # Pleated curtain gathered at one end, with individual suspension rings.
    curtain(level,.64,1.2,(-.44,1.4,.34),'Trim')
    if level<2:
        for i in range(7):
            p=(-.73+i*.10,1.99,.34)
            ring=pivot(c.ring('CurtainRing',p,.025,.019,.009,'Chrome',16),p)
            ring.rotation_euler.y=math.pi/2

def curtain(level,width=1.0,height=1.55,origin=(0,0,0),mat='Curtain'):
    steps=48 if level==0 else 24
    verts=[]
    for y in [-height/2,height/2]:
        for i in range(steps+1):
            u=i/steps
            verts.append((origin[0]+(u-.5)*width,origin[1]+y,origin[2]+math.sin(u*math.tau*8)*.035))
    faces=[(i,i+1,steps+2+i,steps+1+i) for i in range(steps)]
    o=c.mesh('PleatedFabric',verts,faces,mat)
    # Cloth is a closed thin shell, avoiding invisible reverse faces.
    mod=o.modifiers.new('ClothThickness','SOLIDIFY');mod.thickness=.003
    bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=mod.name)
    for p in o.data.polygons:p.use_smooth=True

def cabinet(level):
    box('RecessedDoor',(0,0,0),(.55,.64,.022),'Cabinet',.006)
    for x in [-.247,.247]:box('Stile',(x,0,.018),(.055,.64,.03),'Oak',.004)
    for y in [-.293,.293]:box('Rail',(0,y,.018),(.45,.055,.03),'Oak',.004)
    rod('Pull',(.18,.13,.06),(.18,.25,.06),.008,'Brass')
    for y in [.13,.25]:rod('PullMount',(.18,y,.025),(.18,y,.06),.008,'Brass')

def books(level):
    for i in range(4):
        y=.017+i*.038
        cover='Bedding' if i%2 else 'OakDark'
        box('Pages',(.015*i,y,0),(.22,.028,.16),'Pillow',.002)
        for dy in [-.017,.017]:box('ClothCover',(.015*i,y+dy,0),(.23,.005,.17),cover,.001)
        box('Spine',(-.098+.015*i,y,0),(.01,.036,.17),cover,.002)

def crockery(level):
    for i in range(3):
        c.cylinder('Plate',(0,.012+i*.018,0),.12,.016,'Porcelain',32 if level==0 else 16)
        c.ring('PlateRim',(0,.022+i*.018,0),.118,.10,.008,'Porcelain',32 if level==0 else 16)
    c.ring('Mug',(.21,.065,.02),.045,.037,.12,'Porcelain',32 if level==0 else 16)
    c.cylinder('MugBottom',(.21,.009,.02),.042,.012,'Porcelain',24)
    handle=pivot(c.ring('MugHandle',(.265,.07,.02),.034,.023,.012,'Porcelain',20),(.265,.07,.02))
    handle.rotation_euler.x=math.pi/2

def vent(level):
    box('VentFrame',(0,0,0),(.42,.22,.025),'Trim',.006)
    box('DarkRecess',(0,0,.014),(.37,.17,.008),'DarkMetal',.002)
    for i in range(7):box('Louvre',(0,-.072+i*.024,.024),(.36,.012,.014),'Trim',.002)
    for x in [-.192,.192]:
        for y in [-.085,.085]:box('Screw',(x,y,.016),(.01,.01,.004),'Chrome',.001)

def outlet(level):
    box('Plate',(0,0,0),(.08,.125,.012),'Trim',.006)
    for y in [-.027,.027]:
        pillow('Socket',(0,y,.008),(.046,.044,.008),'Porcelain',level)
        for x in [-.01,.01]:box('PinSlot',(x,y+.004,.014),(.004,.012,.002),'DarkMetal',.0005)
        box('EarthSlot',(0,y-.011,.014),(.005,.005,.002),'DarkMetal',.0005)

def radiator(level):
    box('Backing',(0,.17,0),(1.2,.34,.08),'DarkMetal')
    for i in range(18 if level<2 else 9):
        x=-.56+i*(1.12/(17 if level<2 else 8))
        box('EnamelFin',(x,.17,.04),(.025,.29,.10),'Trim',.006)
    for x in [-.61,.61]:box('EndCap',(x,.17,0),(.06,.34,.13),'Trim')

def frame(level):
    box('Backing',(0,0,0),(.6,.44,.025),'OakDark',.005)
    box('MatBoard',(0,0,.015),(.54,.38,.006),'Pillow',.001)
    # Small original geometric landscape composed of inset coloured relief layers.
    box('Sky',(0,.025,.02),(.43,.26,.004),'Bedding',.001)
    box('Horizon',(0,-.07,.024),(.43,.07,.004),'SofaFabric',.001)
    for i in range(5):box('Woodland',(-.16+i*.08,-.01,.028),(.018,.17-abs(i-2)*.025,.003),'OakDark',.001)
    for x in [-.285,.285]:box('FrameRail',(x,0,.025),(.03,.44,.035),'Oak',.006)
    for y in [-.205,.205]:box('FrameRail',(0,y,.025),(.54,.03,.035),'Oak',.006)

def chair(level):
    pillow('Seat',(0,.45,0),(.44,.055,.44),'Oak',level)
    for x in [-.18,.18]:
        for z in [-.18,.18]:
            rod('TurnedLeg',(x,.025,z),(x*.93,.43,z*.93),.024,'Oak',10 if level else 16)
        rod('BackPost',(x,.42,-.19),(x,.98,-.23),.022,'OakDark')
    for x in [-.12,-.06,0,.06,.12]:rod('BackSpindle',(x,.52,-.20),(x,.94,-.23),.009,'Oak')
    box('CrestRail',(0,.98,-.23),(.44,.065,.055),'OakDark')
    for z in [-.17,.17]:rod('Stretcher',(-.18,.18,z),(.18,.18,z),.012,'Oak')

def dresser(level,night=False):
    w=.46 if night else 1.0; h=.55 if night else .85; d=.4 if night else .5
    for x in [-w/2+.055,w/2-.055]:
        for z in [-d/2+.045,d/2-.045]:box('Foot',(x,.055,z),(.065,.11,.065),'OakDark',.012)
    box('Carcass',(0,h*.52,0),(w-.025,h-.13,d-.02),'Oak')
    box('OverhangingTop',(0,h-.017,0),(w,.034,d),'Oak',.009)
    count=2 if night else 3
    dh=(h-.16)/count
    for i in range(count):
        y=.11+(i+.5)*dh
        box('DrawerFront',(0,y,d/2),(w-.045,dh-.014,.026),'OakDark',.004)
        box('InsetField',(0,y,d/2+.018),(w-.10,dh-.055,.012),'Cabinet',.004)
        for x in ([0] if night else [-w*.27,w*.27]):
            rod('Handle',(x-.04,y,d/2+.055),(x+.04,y,d/2+.055),.007,'Brass')
            for dx in [-.04,.04]:rod('HandleMount',(x+dx,y,d/2+.025),(x+dx,y,d/2+.055),.007,'Brass')

def tree(level):
    import random
    rng=random.Random(1225)
    rod('Trunk',(0,.2,0),(0,2.12,0),.035,'Bark')
    layers=15 if level==0 else 10
    spokes=18 if level==0 else 12
    for j in range(layers):
        y=.42+j*1.65/layers; radius=.58*(1-(y-.35)/1.9)
        for i in range(spokes):
            a=i*math.tau/spokes+j*.71; r=radius*(.84+rng.random()*.16)
            verts=[(0,y+.12,0),(math.cos(a-.16)*r*.82,y-.025,math.sin(a-.16)*r*.82),
                   (math.cos(a)*r,y-.045,math.sin(a)*r),
                   (math.cos(a+.16)*r*.82,y-.025,math.sin(a+.16)*r*.82),
                   (math.cos(a)*r*.46,y+.055,math.sin(a)*r*.46)]
            c.mesh('FirBough',verts,[(0,1,4),(1,2,4),(2,3,4),(3,0,4),(0,3,2,1)],'TreeGreen')

for key,maker in [('Sofa',sofa),('Armchair',lambda l:sofa(l,True)),('Bed',bed),('Toilet',toilet),('Sink',basin),('VanityBasin',lambda l:basin(l,True)),('Tub',tub),('Curtain',curtain),('CabinetDoor',cabinet),('Books',books),('Crockery',crockery),('Vent',vent),('Outlet',outlet),('Radiator',radiator),('Picture',frame),('Chair',chair),('Dresser',dresser),('Nightstand',lambda l:dresser(l,True)),('TreeFoliage',tree)]:
    if ONLY and key not in ONLY: continue
    c.export(key,maker,lod=True)
print('LARCH_COURT_COMPLETE',OUT)
