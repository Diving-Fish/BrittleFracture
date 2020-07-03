﻿

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TetrahedronNodes3D = ElementNS.Tuple4D<int>;
using TetrahedronEdges3D = ElementNS.Tuple6D<int>;
using Node3D = UnityEngine.Vector3;
using Edge3D = EdgeNS.Edge;

public class VolumeticMesh3D
{
    /// <summary>
    /// default constructor
    /// </summary>
    public VolumeticMesh3D()
    {
        // this.volume = new List<double>();
        this.edges = new List<Edge3D>();
        this.nodes = new List<Node3D>();
        this.damages = new List<Damage3D>();

        this.nodeIndexOfTetra = new List<TetrahedronNodes3D>();
        this.edgeIndexOfTetra = new List<TetrahedronEdges3D>();
    }

    /// <summary>
    /// VM - N
    /// containing 4 node indexes of a tetra.
    /// </summary>
    public List<TetrahedronNodes3D> nodeIndexOfTetra;

    /// <summary>
    /// VM - L
    /// containing 6 edge indexes of a tetra.
    /// </summary>
    public List<TetrahedronEdges3D> edgeIndexOfTetra;

    // @ all lists above contains index values
    // @ all lists below contains literal values

    /// <summary>
    /// VM - E
    /// calculate the volume of a specific tetra
    /// </summary>
    /// <param name="index">calculate which tetra's volume?</param>
    public float getVolumeOfTetraAtIndex(int index)
    {
        var points = nodeIndexOfTetra[index];
        var edge1 = nodes[points.b] - nodes[points.a];
        var edge2 = nodes[points.c] - nodes[points.a];
        var edge3 = nodes[points.d] - nodes[points.a];

        return Vector3.Dot(edge1, Vector3.Cross(edge2, edge3));
    }

    /// <summary>
    /// nodes
    /// do not Add to nodes directly
    /// use wrapped function `tryAddNode` instead
    /// </summary>
    public List<Node3D> nodes;

    /// <summary>
    /// edges
    /// do not Add to edges directly
    /// use wrapped function `tryAddEdge` instead
    /// </summary>
    public List<Edge3D> edges;


    /// <summary>
    /// damages
    /// do not Add to damages directly
    /// use wrapped function `tryAddDamage` instead
    /// </summary>
    public List<Damage3D> damages;


    /// <summary>
    /// calculateVolumes
    /// calculate the whole volume of all tetras
    /// </summary>
    public float calculateVolumes()
    {
        float sumVolume = 0;
        for (int i = 0; i < nodeIndexOfTetra.Count; ++i)
        {
            sumVolume += getVolumeOfTetraAtIndex(i);
        }
        return sumVolume;
    }

    /// <summary>
    /// tryAddNode
    /// find the node in current nodes.
    /// if exists, return its index
    /// if not, insert it and return its new index
    /// </summary>
    /// <param name="node">the node to insert or find</param>
    public int tryAddNode(Node3D node)
    {
        var index = nodes.FindIndex(nod => nod == node);
        if (index == -1)
        {
            nodes.Add(node);
            return nodes.Count - 1;
        }
        else
        {
            return index;
        }
    }

    /// <summary>
    /// tryAddNode
    /// find the node in current nodes.
    /// if exists, return its index
    /// if not, insert it and return its new index
    /// </summary>
    /// <param name="from">one tail of the edge. choose which tail is irrelevant.</param>
    /// <param name="to">one tail of the edge. choose which tail is irrelevant.</param>
    public int tryAddEdge(int from, int to)
    {
        var edge = new Edge3D(from, to);
        return tryAddEdge(edge);
    }

    /// <summary>
    /// tryAddEdge
    /// find the edge in current edges.
    /// if exists, return its index
    /// if not, insert it and return its new index
    /// </summary>
    /// <param name="edge">the edge to insert or find</param>
    public int tryAddEdge(Edge3D edge)
    {
        var index = edges.FindIndex(edg => edg == edge);
        if (index == -1)
        {
            edges.Add(edge);
            return edges.Count - 1;
        }
        else
        {
            return index;
        }
    }

    /// <summary>
    /// addTetrahedron
    /// add a tetrahedron into current mesh
    /// automatically adds nodes and edges
    /// and sets nodeIndexOfTetra and edgeIndexOfTetra
    /// </summary>
    /// <param name="nodeA">one node of four constructing the tetrahedron</param>
    /// <param name="nodeB">one node of four constructing the tetrahedron</param>
    /// <param name="nodeC">one node of four constructing the tetrahedron</param>
    /// <param name="nodeD">one node of four constructing the tetrahedron</param>
    public int addTetrahedron(Node3D nodeA, Node3D nodeB, Node3D nodeC, Node3D nodeD)
    {
        var nodeIndexA = tryAddNode(nodeA);
        var nodeIndexB = tryAddNode(nodeB);
        var nodeIndexC = tryAddNode(nodeC);
        var nodeIndexD = tryAddNode(nodeD);

        var edgeA = tryAddEdge(nodeIndexA, nodeIndexB);
        var edgeB = tryAddEdge(nodeIndexA, nodeIndexC);
        var edgeC = tryAddEdge(nodeIndexA, nodeIndexD);
        var edgeD = tryAddEdge(nodeIndexB, nodeIndexC);
        var edgeE = tryAddEdge(nodeIndexB, nodeIndexD);
        var edgeF = tryAddEdge(nodeIndexC, nodeIndexD);

        // Debug.Log(string.Format("{0} {1} {2} {3} {4} {5}", edgeA, edgeB, edgeC, edgeD, edgeE, edgeF));

        nodeIndexOfTetra.Add(new TetrahedronNodes3D(nodeIndexA, nodeIndexB, nodeIndexC, nodeIndexD));
        edgeIndexOfTetra.Add(new TetrahedronEdges3D(edgeA, edgeB, edgeC, edgeD, edgeE, edgeF));

        if (nodeIndexOfTetra.Count != edgeIndexOfTetra.Count)
        {
            Debug.LogError("volmesh3D internal inconsistency");
        }
        return nodeIndexOfTetra.Count - 1;
    }

    /// <summary>
    /// tryAddDamage
    /// find the damage in current damages.
    /// if exists, return its index
    /// if not, insert it and return its new index
    /// </summary>
    /// <param name="damage">the damage to insert or find</param>
    public int tryAddDamage(Damage3D damage)
    {
        var index = damages.FindIndex(damag => damag == damage);

        if (index == -1)
        {
            damages.Add(damage);
            return damages.Count - 1;
        }
        else
        {
            return index;
        }
    }

    /// <summary>
    /// isDamagedEdge
    /// judge if an edge is damaged
    /// </summary>
    /// <param name="edgeIndex">the edge index in current edges List</param>
    public bool isDamagedEdge(int edgeIndex)
    {
        var edge = edges[edgeIndex];
        foreach (var damage in damages)
        {
            if (damage.edge.Equals(edge))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// isDamagedEdge
    /// judge if an edge is damaged
    /// </summary>
    /// <param name="edge">the edge to be judged</param>
    public bool isDamagedEdge(Edge3D edge)
    {
        foreach (var damage in damages)
        {
            if (damage.edge.Equals(edge))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// isDamagedTetrahedron
    /// judge if an tetrahedron contains damaged edge
    /// </summary>
    /// <param name="tetraIndex">the index of tetrahedron to be judged</param>
    public bool isDamagedTetrahedron(int tetraIndex)
    {
        foreach (var edge in edgeIndexOfTetra[tetraIndex])
        {
            foreach (var damage in damages)
            {
                if (damage.edge.Equals(edge))
                {
                    return true;
                }
            }
        }
        return false;
    }


    /// <summary>
    /// getCloseNeighbors
    /// returns all nodes index that directly connects to provided tetrahedron
    /// </summary>
    /// <param name="index">the index of node to be calculated</param>
    public List<int> getNeighborNodes(int index)
    {
        var neighbors = new HashSet<int>();
        foreach (var edge in edges)
        {
            if (edge.from == index)
            {
                neighbors.Add(edge.to);
            }
            else if (edge.to == index)
            {
                neighbors.Add(edge.from);
            }
        }
        return new List<int>(neighbors);
    }

    /// <summary>
    /// getCloseNeighbors
    /// returns all tetrahedron index that directly connects to provided tetrahedron
    /// </summary>
    /// <param name="index">the index of tetrahedron to be calculated</param>
    public List<int> getNeighborTetras(int index)
    {
        var targetNodes = new List<int>();

        foreach (var i in nodeIndexOfTetra[index].flatten())
        {
            targetNodes.Add(i);
        }

        var resultIndex = new List<int>();
        for (int i = 0; i < edgeIndexOfTetra.Count; ++i)
        {
            if (i == index) continue;

            int counter = 0;

            foreach (var node in nodeIndexOfTetra[i].flatten())
            {
                if (targetNodes.Contains(node)) ++counter;
            }

            if (counter > 2)
            {
                resultIndex.Add(i);
            }
        }

        return resultIndex;
    }


    /// <summary>
    /// getHitRespondingTetraIndex
    /// returns the responding tetrahedron index
    /// </summary>
    /// <param name="position">the world position where the hit happens</param>
    public int getHitRespondingTetraIndex(Vector3 position)
    {
        float minDistance = float.MaxValue;
        int minDistanceTetraIndex = -1;

        for (int i = 0; i < nodeIndexOfTetra.Count; ++i)
        {
            var nodesTuple = nodeIndexOfTetra[i];
            float distance = 0;

            foreach (var node in nodesTuple)
            {
                distance += (nodes[(int)node] - position).sqrMagnitude;
            }

            if (minDistance > distance)
            {
                minDistance = distance;
                minDistanceTetraIndex = i;
            }
        }

        return minDistanceTetraIndex;
    }

    /// <summary>
    /// implicitSurface
    /// caculate if a point is on this implicit surface
    /// </summary>
    /// <param name="pos">the position of the point to be judged</param>
    /// <param name="pos0">the center of the element e0</param>
    /// <param name="direction">the maximum principal stress of the element e0</param>
    public float implicitSurface(Node3D pos, Node3D pos0, Vector3 direction)
    {
        direction.Normalize();

        Vector3 va, vc;

        if (direction.x != 0 && direction.y != 0)
        {
            va = new Vector3(0, direction.z, -direction.y);
            vc = new Vector3(direction.z, 0, -direction.x);
        }
        else if (direction.x != 0 && direction.z != 0)
        {
            va = new Vector3(0, direction.z, -direction.y);
            vc = new Vector3(direction.y, -direction.x, 0);
        }
        else if (direction.y != 0 && direction.z != 0)
        {
            va = new Vector3(direction.z, 0, -direction.x);
            vc = new Vector3(direction.y, -direction.x, 0);
        }
        else if (direction.x != 0)
        {
            va = new Vector3(0, 1, 0);
            vc = new Vector3(0, 0, 1);
        }
        else if (direction.y != 0)
        {
            va = new Vector3(1, 0, 0);
            vc = new Vector3(0, 0, 1);
        }
        else if (direction.z != 0)
        {
            va = new Vector3(1, 0, 0);
            vc = new Vector3(0, 1, 0);
        }
        else
        {
            return 0;
        }

        var offset = pos - pos0;
        var R = Matrix3D.initMatrixWithColumnVectors(va.normalized, direction, vc.normalized);

        var result = R.multiply(offset);

        return result.y - Mathf.PerlinNoise(result.x, result.z);
    }

    /// <summary>
    /// isCrossed
    /// judge if the element is crossed by fracture surface
    /// split edge which cross the fracture surface
    /// </summary>
    /// <param name="pos0">the center of the element e0</param>
    /// <param name="direction">the maximum principal stress of the element e0</param>
    /// <param name="tetra"> the element to be judged</param>
    public bool isCrossed(Node3D pos0, Vector3 direction, int tetra)
    {
        // Debug.Log(tetra);
        // Debug.Log(edgeIndexOfTetra[tetra].a);
        // Debug.Log(edgeIndexOfTetra[tetra].b);
        // Debug.Log(edgeIndexOfTetra[tetra].c);
        // Debug.Log(edgeIndexOfTetra[tetra].d);
        // Debug.Log(edgeIndexOfTetra[tetra].e);
        // Debug.Log(edgeIndexOfTetra[tetra].f);

        List<int> edgeOfTetra = new List<int>();
        edgeOfTetra.Add(edgeIndexOfTetra[tetra].a);
        edgeOfTetra.Add(edgeIndexOfTetra[tetra].b);
        edgeOfTetra.Add(edgeIndexOfTetra[tetra].c);
        edgeOfTetra.Add(edgeIndexOfTetra[tetra].d);
        edgeOfTetra.Add(edgeIndexOfTetra[tetra].e);
        edgeOfTetra.Add(edgeIndexOfTetra[tetra].f);

        bool crossed = false;
        foreach (var i in edgeOfTetra)
        {
            // Debug.Log(i); Debug.Log(edges[i].from); Debug.Log("Bar");
            float fromPos = implicitSurface(nodes[edges[i].from], pos0, direction);
            float toPos = implicitSurface(nodes[edges[i].to], pos0, direction);
            if (fromPos * toPos <= 0)
            {
                float cutPos = Mathf.Abs(fromPos) / (Mathf.Abs(fromPos) + Mathf.Abs(toPos));
                tryAddDamage(new Damage3D(edges[i], cutPos));
                crossed = true;
            }
        }

        return crossed;
    }

    /// <summary>
    /// propagatingCracks
    /// propagate the fracture, split edge which cross the fracture surface
    /// </summary>
    /// <param name="hitPos">the hit point</param>
    /// <param name="initialDirection">the maximum principal stress of the element e0</param>
    /// <param name="fractureToughness">fracture toughness of the material</param>
    /// <param name="constantFactor">constant factor that links Ef and Es</param>
    /// <param name="strainEnergyDensity">the strain energy density of element e</param>
    public void propagatingCracks(Vector3 hitPos,
        Vector3 initialDirection,
        float fractureToughness,
        float constantFactor,
        float strainEnergyDensity)
    {
        // Debug.Log(nodeIndexOfTetra.Count); //3072
        // Debug.Log(edgeIndexOfTetra.Count); //3072
        // Debug.Log(nodes.Count);            //729
        // Debug.Log(edges.Count);            //4856
        // Debug.Log(damages.Count);

        // some value that cannot caculate now
        float areaOfFractureSurfaceThatCrossesElement = 1.0f;

        int hitTetra = getHitRespondingTetraIndex(hitPos);

        List<int> nodesOfTetra = new List<int>();
        nodesOfTetra.Add(nodeIndexOfTetra[hitTetra].a);
        nodesOfTetra.Add(nodeIndexOfTetra[hitTetra].b);
        nodesOfTetra.Add(nodeIndexOfTetra[hitTetra].c);
        nodesOfTetra.Add(nodeIndexOfTetra[hitTetra].d);

        Node3D center0 = new Node3D(0.0f, 0.0f, 0.0f);
        foreach (var i in nodesOfTetra)
        {
            center0 += nodes[i];
        }
        center0 /= 4.0f;

        List<int> fracTetra = new List<int>();
        List<int> nextTetra = new List<int>();
        List<int> tmpTetra = new List<int>();

        fracTetra.Add(hitTetra);
        tmpTetra.Add(hitTetra);

        float fracEnergy = 0.0f;
        float strainEnergy = 0.0f;

        while (tmpTetra.Count != 0)
        {
            foreach (var tetra in tmpTetra)
            {
                fracEnergy +=
                    areaOfFractureSurfaceThatCrossesElement *
                    fractureToughness;
                strainEnergy +=
                    areaOfFractureSurfaceThatCrossesElement *
                    constantFactor *
                    strainEnergyDensity;

                // update frac Edges

                foreach (var neTetra in getNeighborTetras(tetra))
                {
                    if (isCrossed(center0, initialDirection, neTetra) &&
                        fracEnergy < strainEnergy &&
                        !fracTetra.Contains(neTetra))
                    {
                        fracTetra.Add(neTetra);
                        nextTetra.Add(neTetra);
                    }
                }
            }
            tmpTetra = nextTetra;
            nextTetra.Clear();
        }
    }
}