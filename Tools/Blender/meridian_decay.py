"""Fixed abandoned set dressing; no pickups, collision or real-world brands."""
import math, random
from meridian_core import box, mesh, rod, cylinder, export

def patch(name, key, radius=(1.2,.8), seed=1):
    rng=random.Random(seed); verts=[(0,.005,0)]
    for i in range(28):
        a=i*math.tau/28; r=rng.uniform(.83,1.0)
        verts.append((math.cos(a)*radius[0]*r,.005,math.sin(a)*radius[1]*r))
    return mesh(name,verts,[(0,i+1,(i+1)%28+1) for i in range(28)],key)

def rubble(lod=0):
    rng=random.Random(1327)
    for i in range((16,8,3)[lod]):
        x,z=rng.uniform(-.9,.9),rng.uniform(-.6,.6)
        rx,rz,h=rng.uniform(.12,.34),rng.uniform(.12,.29),rng.uniform(.08,.25)
        vertices=[(x-rx,.01,z-rz),(x+rx,.01,z-rz),(x+rx,.01,z+rz),(x-rx,.01,z+rz),
                  (x-rx*.7,h,z-rz*.6),(x+rx*.5,h*.8,z-rz*.7),
                  (x+rx*.7,h*.9,z+rz*.5),(x-rx*.8,h*.75,z+rz*.7)]
        mesh('CollapsedConcrete',vertices,[(0,3,2,1),(4,5,6,7),(0,1,5,4),
             (1,2,6,5),(2,3,7,6),(3,0,4,7)],'Concrete')
    if lod<2:
        for i in range(3):box('CeilingFragment',(-.5+i*.45,.045,.2),(.44,.045,.28),'Ceiling',.004,rot=i*22)

def broken_glass(lod=0):
    rng=random.Random(1312)
    for i in range((18,9,3)[lod]):
        x,z=rng.uniform(-.8,.8),rng.uniform(-.5,.5)
        mesh('GlassShard',[(x,.008,z),(x+rng.uniform(.07,.24),.014,z+.035),
             (x+.03,.01,z+rng.uniform(.1,.25))],[(0,1,2)],'Glass')

def rebar(lod=0):
    for i in range(5 if lod<2 else 3):
        x=-.75+i*.35
        rod('TornRebar',(x,.025,-.4),(x-.08,.49,.2),.018,'Rust',8)
        rod('BentEnd',(x-.08,.49,.2),(x+.07,.57,.40),.018,'Rust',8)
    if lod==0:
        for z in (-.3,0,.3):rod('RebarTie',(-.9,.075,z),(.9,.075,z),.012,'Rust',6)

def slab_edge(lod=0):
    vertices=[(-.8,0,-.45),(.8,0,-.45),(.8,0,.16),(.46,0,.22),(.30,0,.45),(-.13,0,.20),(-.57,0,.36),(-.8,0,.10)]
    n=len(vertices); vertices += [(x,y-.22,z) for x,y,z in vertices]
    faces=[tuple(range(n)),tuple(range(2*n-1,n-1,-1))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    mesh('RupturedSlab',vertices,faces,'Concrete')
    if lod<2:
        for i in range(4):rod('ExposedRod',(-.6+i*.36,-.07,.12),(-.66+i*.36,-.02,.57),.014,'Rust',6)

def plant(lod=0):
    count=(9,5,2)[lod]; rng=random.Random(1387)
    for i in range(count):
        x,z=rng.uniform(-.6,.6),rng.uniform(-.4,.4); h=rng.uniform(.30,.85)
        rod('WildStem',(x,0,z),(x+.08,h,z+.06),.009,'BoardWood',6)
        for j in range(3 if lod<2 else 1):
            y=h*(.35+j*.22); dx=.17*(1 if j%2 else -1)
            mesh('BroadLeaf',[(x,y,z),(x+dx*.4,y+.025,z-.055),(x+dx,y+.045,z+.035),
                 (x+dx*.4,y+.025,z+.09)],[(0,1,2,3)],'Moss')

def leaves(lod=0):
    rng=random.Random(1344)
    for i in range((28,13,4)[lod]):
        x,z=rng.uniform(-.95,.95),rng.uniform(-.7,.7); a=rng.uniform(0,math.tau)
        dx,dz=math.cos(a)*.12,math.sin(a)*.12
        mesh('BlownLeaf',[(x-dx,.009,z-dz),(x-dz*.4,.018,z+dx*.4),
             (x+dx,.014,z+dz),(x+dz*.4,.009,z-dx*.4)],[(0,1,2,3)],'Leaves')

def mattress(lod=0):
    box('DiscardedMattress',(0,.12,0),(1.22,.24,2.0),'Upholstery',.085,detail=3-lod)
    if lod<2:
        for x in (-.61,.61):box('TapedEdge',(x,.135,0),(.012,.035,1.86),'BoardWood',.005)
        for z in (-.97,.97):box('TapedEdge',(0,.135,z),(1.10,.035,.015),'BoardWood',.005)
    if lod==0:
        for x in (-.4,0,.4):
            for z in (-.75,-.25,.25,.75):cylinder('Tuft',(x,.242,z),.03,.008,'FixtureTrim',8)

def boarded(lod=0):
    for i in range(8):box('Board',(0,.24+i*.36,0),(3.8,.31,.055),'BoardWood',.01,rot=2 if i%2 else -2)
    for x in (-1.45,1.45):box('Brace',(x,1.51,-.04),(.14,2.92,.07),'BoardWood',.012)
    if lod==0:
        for x in (-1.45,1.45):
            for y in (.25,.96,1.69,2.40):rod('Nail',(x,y,.029),(x,y,.045),.009,'Rust',8)

def sign_frame(lod=0):
    box('SignBacking',(0,.31,0),(2.4,.62,.085),'StorefrontTrim',.02)
    box('InsetFace',(0,.31,.048),(2.26,.48,.018),'Plaster',.006)
    for y in (.035,.585):box('BrassEdge',(0,y,.065),(2.33,.025,.025),'StorefrontTrim',.004)

def poster(lod=0):
    box('TornNotice',(0,.40,0),(.54,.8,.012),'Plaster',0)
    box('PortraitSilhouette',(0,.45,.012),(.14,.25,.008),'FixtureTrim',.035,detail=2-lod//2)
    if lod<2:
        for y in (.11,.17,.68):box('FadedTypeRule',(0,y,.014),(.38,.012,.006),'FixtureTrim',0)

def generate():
    makers={'DebrisPile':rubble,'Rubble':rubble,'BrokenGlass':broken_glass,'ExposedRebar':rebar,
        'BrokenSlabEdge':slab_edge,'Overgrowth':plant,'LeafScatter':leaves,'CampMattress':mattress,
        'BoardedPanel':boarded,'SignFrame':sign_frame,'MissingPoster':poster,
        'Puddle':lambda l:patch('StandingWater','Water'),
        'CampScorch':lambda l:patch('OldCampfireScorch','Scorch',(1.0,.8),9),
        'GraffitiTag':lambda l:mesh('OriginalAbstractTag',[(-1,0,-.6),(1,0,-.6),(1,0,.6),(-1,0,.6)],[(0,1,2,3)],'Graffiti'),
        'MossPatch':lambda l:patch('LeakDrivenMoss','Moss',(.9,.55),19),
        'WaterStain':lambda l:patch('TideStain','Scorch',(.9,.55),3)}
    for name,maker in makers.items():export(name,maker)
