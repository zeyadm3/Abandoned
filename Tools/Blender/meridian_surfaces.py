"""Separate floor finish, damage and pre-fracture assets for readable structure."""
import math, random
from meridian_core import box, mesh, export, export_chunks

SURFACES = ('Terrazzo','Tile','Carpet','Concrete','Wood','Bridge')

def slab_polygon(name, polygon, top, bottom, key):
    n=len(polygon)
    vertices=[(x,top,z) for x,z in polygon]+[(x,bottom,z) for x,z in polygon]
    faces=[tuple(range(n)),tuple(range(2*n-1,n-1,-1))]
    faces += [(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    return mesh(name,vertices,faces,key)

def sag_slab(name, top, thickness, key, sag):
    n=8; vertices=[]
    for below in (0,thickness):
        for z in range(n+1):
            for x in range(n+1):
                px,pz=-2+x*4/n,-2+z*4/n
                py=top-sag*math.sin(x*math.pi/n)*math.sin(z*math.pi/n)-below
                vertices.append((px,py,pz))
    faces=[]; offset=(n+1)**2
    for z in range(n):
        for x in range(n):
            a=z*(n+1)+x; b=a+1; c=b+n+1; d=a+n+1
            faces += [(a,b,c,d),(d+offset,c+offset,b+offset,a+offset)]
    edges=list(range(n+1))+[z*(n+1)+n for z in range(1,n+1)]
    edges += list(range(offset-2,offset-n-2,-1))+[z*(n+1) for z in range(n-1,0,-1)]
    for i,a in enumerate(edges):
        b=edges[(i+1)%len(edges)]; faces.append((a,a+offset,b+offset,b))
    return mesh(name,vertices,faces,key)

def crack_path(name, points, width=.018):
    vertices=[]
    for i,p in enumerate(points):
        q=points[min(i+1,len(points)-1)] if i<len(points)-1 else points[i-1]
        dx,dz=q[0]-p[0],q[1]-p[1]; length=max(.001,math.hypot(dx,dz))
        vertices += [(p[0]-dz/length*width,.004,p[1]+dx/length*width),
                     (p[0]+dz/length*width,.004,p[1]-dx/length*width)]
    return mesh(name,vertices,[(i*2,i*2+1,i*2+3,i*2+2) for i in range(len(points)-1)],'Crack')

def floor(surface, stage=0, lod=0):
    key='BridgeMetal' if surface=='Bridge' else surface
    if stage==2:
        sag_slab('SaggingCore',-.025,.225,'Concrete',.13)
        sag_slab('SaggingFinish',0,.025,key,.13)
    else:
        box('ConcreteCore',(0,-.1375,0),(4,.225,4),'Concrete',.008,detail=1)
        box('WalkingFinish',(0,-.0125,0),(4,.025,4),key,.002,detail=1)
    # Slab boundary reads in torchlight independently of damage state.
    for x in (-1.992,1.992): box('SectionDivider',(x,.001,0),(.018,.004,4),'StorefrontTrim',0)
    for z in (-1.992,1.992): box('SectionDivider',(0,.001,z),(4,.004,.018),'StorefrontTrim',0)
    if stage:
        rng=random.Random(1300+SURFACES.index(surface))
        points=[(-1.92+i*.48,rng.uniform(-.22,.22)+i*.09) for i in range(9)]
        if stage==2:
            # Follow the sag so fractures do not float above the slab.
            crack=crack_path('OpenedFracture',points,.032)
            for vertex in crack.data.vertices:
                px,pz=vertex.co.x,vertex.co.y
                vertex.co.z-=.13*math.cos(px*math.pi/4)*math.cos(pz*math.pi/4)
        else: crack_path('HairlineFracture',points,.015)
        for i in (2,5,7):
            a=points[i]; branch=crack_path('Branch',[(a[0],a[1]),(a[0]+.11,a[1]+.48),(a[0]-.12,a[1]+.82)],.009)
            if stage==2:
                for vertex in branch.data.vertices:
                    vertex.co.z-=.13*math.cos(vertex.co.x*math.pi/4)*math.cos(vertex.co.y*math.pi/4)

def fractures(surface):
    rng=random.Random(1391); grid={}
    for z in range(4):
        for x in range(4):
            grid[x,z]=(-2+x*4/3+(rng.uniform(-.18,.18) if 0<x<3 else 0),
                       -2+z*4/3+(rng.uniform(-.18,.18) if 0<z<3 else 0))
    chunks=[]; key='BridgeMetal' if surface=='Bridge' else surface
    for z in range(3):
        for x in range(3):
            poly=[grid[x,z],grid[x+1,z],grid[x+1,z+1],grid[x,z+1]]
            def make(p=poly):
                slab_polygon('BrokenConcrete',p,-.023,-.25,'Concrete')
                slab_polygon('BrokenFinish',p,0,-.023,key)
            chunks.append(make)
    export_chunks('Floor'+surface+'Fractured',chunks)

def generate():
    for surface in SURFACES:
        for suffix,stage in (('',0),('Cracked',1),('Sagged',2)):
            export('Floor'+surface+suffix,lambda lod,s=surface,t=stage:floor(s,t,lod))
        fractures(surface)
