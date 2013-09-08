using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class GridVector
{
    public static GridVector plusX = new GridVector(1, 0);
    public static GridVector minusX = new GridVector(-1, 0);
    public static GridVector plusY = new GridVector(0, 1);
    public static GridVector minusY = new GridVector(0, -1);
    public static GridVector zero = new GridVector(0, 0);

    public int x;
    public int y;

    public GridVector minimized
    {
        get
        {
            var m = new GridVector(this);
            if (m.x > 0) m.x = 1;
            if (m.x < 0) m.x = -1;
            if (m.y > 0) m.y = 1;
            if (m.y < 0) m.y = -1;
            return m;
        }
    }

    public GridVector(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public GridVector(GridVector point)
    {
        x = point.x;
        y = point.y;
    }

    public static GridVector operator +(GridVector a, GridVector b)
    {
        return new GridVector(a.x + b.x, a.y + b.y);
    }

    public static GridVector operator -(GridVector a, GridVector b)
    {
        return new GridVector(a.x - b.x, a.y - b.y);
    }

    public static GridVector operator /(GridVector a, int b)
    {
        return new GridVector(a.x / b, a.y / b);
    }

    public static GridVector operator *(GridVector a, int b)
    {
        return new GridVector(a.x * b, a.y * b);
    }

    public static bool operator ==(GridVector a, GridVector b)
    {
        if (System.Object.ReferenceEquals(a, b))
        {
            return true;
        }
        if ((object)a == null || (object)b == null)
        {
            return false;
        }
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(GridVector a, GridVector b)
    {
        return !(a == b);
    }

    public override string ToString()
    {
        return "(" + x + ", " + y + ")";
    }
}

public class RoomWall
{
    public bool canGrow;
    public GridVector start;
    public GridVector end;

    public GridVector outwards
    {
        get { return new GridVector(-direction.y, direction.x); }
    }

    public GridVector direction
    {
        get { return end - start; }
    }

    public int length
    {
        get { return Math.Abs(end.x - start.x) + Math.Abs(end.y - start.y); }
    }

    public RoomWall()
    {
    }

    public RoomWall(GridVector start, GridVector end)
    {
        this.start = start;
        this.end = end;
    }

    public RoomWall(RoomWall wall)
    {
        start = wall.start;
        end = wall.end;
    }

    public static int CompareByLength(RoomWall a, RoomWall b)
    {
        if (a.length > b.length) return 1;
        if (a.length == b.length) return 0;
        return -1;
    }

    public static RoomWall operator +(RoomWall a, GridVector b)
    {
        var result = new RoomWall(a);
        result.start += b;
        result.end += b;
        return result;
    }

    public static RoomWall operator -(RoomWall a, GridVector b)
    {
        var result = new RoomWall(a);
        result.start -= b;
        result.end -= b;
        return result;
    }
}


public class Room
{
    // Примыкающие стены и углы считаются по часовой стрелке, если смотреть из центра комнаты
    public List<GridVector> corners;
    public Color color;
    public bool canSpawn = false;
    public List<RoomWall> walls
    {
        get
        {
            var roomWalls = new List<RoomWall>();
            for (var i = 0; i < corners.Count; i++)
            {
                roomWalls.Add(new RoomWall(corners[i], i == corners.Count - 1 ? corners[0] : corners[i + 1]));
            }
            return roomWalls;
        }
    }
    public int perimeter
    {
        get
        {
            var p = 0;
            foreach (var wall in walls)
            {
                p += wall.length;
            }
            return p;
        }
    }

    public Room(Color color, List<GridVector> corners)
    {
        this.color = color;
        this.corners = corners;
        SortCorners();
    }

    public GridVector SortCorners()
    {
        // Ищем границы комнаты
        var minX = corners[0].x;
        var maxX = corners[0].x;
        var minY = corners[0].y;
        var maxY = corners[0].y;
        foreach (var corner in corners)
        {
            if (corner.x < minX) minX = corner.x;
            if (corner.x > maxX) maxX = corner.x;
            if (corner.y < minY) minY = corner.y;
            if (corner.y > maxY) maxY = corner.y;
        }

        // Сортируем углы комнаты
        var oldC = new List<GridVector>(corners);
        var newC = new List<GridVector>();
        bool parallelX = false;
        while (oldC.Count > 1)
        {
            // Ищем первый угол
            if (newC.Count == 0)
            {
                if (ScanUp(ref oldC, ref newC, minX, minY, maxY)) continue;
                if (ScanRight(ref oldC, ref newC, minX, minY, maxX)) continue;
                if (ScanDown(ref oldC, ref newC, minX, minY, minY)) continue;
                if (!ScanLeft(ref oldC, ref newC, minX, minY, minX))
                {
                    Debug.Log("Error on start");
                    return null;
                }
            }
            // Ищем остальные углы
            else
            {
                var last = newC[newC.Count - 1];
                if (parallelX)
                {
                    if (ScanRight(ref oldC, ref newC, last.x, last.y, maxX))
                    {
                        parallelX = false;
                        continue;
                    }
                    if (ScanLeft(ref oldC, ref newC, last.x, last.y, minX))
                    {
                        parallelX = false;
                        continue;
                    }
                }
                else
                {
                    if (ScanUp(ref oldC, ref newC, last.x, last.y, maxY))
                    {
                        parallelX = true;
                        continue;
                    }
                    if (ScanDown(ref oldC, ref newC, last.x, last.y, minY))
                    {
                        parallelX = true;
                        continue;
                    }
                }
                Debug.Log("Error -------------------------------------------------");
                Debug.Log("Corners: " + corners.Count);
                Debug.Log("OldC: " + oldC.Count);
                Debug.Log("NewC: " + newC.Count);
                Debug.Log(last);
                color = Color.red;
                return last;
            }
        }
        // Добавляем последний оставшийся угол
        newC.Add(oldC[0]);
        corners = newC;
        return null;
    }

    bool ScanLeft(ref List<GridVector> oldC, ref List<GridVector> newC, int startX, int startY, int minX)
    {
        for (var x = startX; x >= minX; x--)
        {
            var index = oldC.FindIndex(gv => gv.x == x && gv.y == startY);
            if (index > -1)
            {
                newC.Add(oldC[index]);
                oldC.RemoveAt(index);
                return true;
            }
        }
        return false;
    }

    bool ScanUp(ref List<GridVector> oldC, ref List<GridVector> newC, int startX, int startY, int maxY)
    {
        for (var y = startY; y <= maxY; y++)
        {
            var index = oldC.FindIndex(gv => gv.x == startX && gv.y == y);
            if (index > -1)
            {
                newC.Add(oldC[index]);
                oldC.RemoveAt(index);
                return true;
            }
        }
        return false;
    }

    bool ScanRight(ref List<GridVector> oldC, ref List<GridVector> newC, int startX, int startY, int maxX)
    {
        for (var x = startX; x <= maxX; x++)
        {
            var index = oldC.FindIndex(gv => gv.x == x && gv.y == startY);
            if (index > -1)
            {
                newC.Add(oldC[index]);
                oldC.RemoveAt(index);
                return true;
            }
        }
        return false;
    }

    bool ScanDown(ref List<GridVector> oldC, ref List<GridVector> newC, int startX, int startY, int minY)
    {
        for (var y = startY; y >= minY; y--)
        {
            var index = oldC.FindIndex(gv => gv.x == startX && gv.y == y);
            if (index > -1)
            {
                newC.Add(oldC[index]);
                oldC.RemoveAt(index);
                return true;
            }
        }
        return false;
    }

    public void GrowWall(RoomWall wall)
    {
        for (var i = 0; i < corners.Count; i++)
        {
            if (i < corners.Count - 1)
            {
                if (corners[i] == wall.start && corners[i + 1] == wall.end)
                {
                    corners[i] += wall.outwards.minimized;
                    corners[i + 1] += wall.outwards.minimized;
                    return;
                }
                if (corners[i] == wall.end && corners[i + 1] == wall.start)
                {
                    corners[i] -= wall.outwards.minimized;
                    corners[i + 1] -= wall.outwards.minimized;
                    return;
                }
            }
            else
            {
                if (corners[i] == wall.start && corners[0] == wall.end)
                {
                    corners[i] += wall.outwards.minimized;
                    corners[0] += wall.outwards.minimized;
                    return;
                }
                if (corners[i] == wall.end && corners[0] == wall.start)
                {
                    corners[i] -= wall.outwards.minimized;
                    corners[0] -= wall.outwards.minimized;
                    return;
                }
            }
        }
        for (var i = 0; i < corners.Count; i++)
        {
            if (corners[i] == wall.start)
            {
                corners.Add(wall.end);
                corners.Add(wall.end);
                SortCorners();
                GrowWall(wall);
                return;
            }
            if (corners[i] == wall.end)
            {
                corners.Add(wall.start);
                corners.Add(wall.start);
                SortCorners();
                GrowWall(wall);
                return;
            }
        }
        corners.Add(wall.start);
        corners.Add(wall.start);
        corners.Add(wall.end);
        corners.Add(wall.end);
        SortCorners();
        GrowWall(wall);
    }

    public static int CompareByPerimeter(Room a, Room b)
    {
        if (a.perimeter > b.perimeter) return 1;
        if (a.perimeter == b.perimeter) return 0;
        return -1;
    }
}


public class FloorPlanGenerator : MonoBehaviour
{
    public Texture2D testWalls;
    public Texture2D transparent;
    public Renderer scanRenderer;
    public bool growCorners;
    public bool growCenters;
    public bool capture = true;
    public bool paused;

    private Texture2D texture;
    private Texture2D scanTexture;
    private Transform tr;
    private Camera cam;
    private RaycastHit selected;
    private System.Random random;
    private List<Room> rooms;
    private int growableRooms;
    private int growableCorners;
    private int bigCorner;
    private int bigCenter;
    private int screenshotCount;
    private Room bigCornerRoom;
    private Room bigCenterRoom;
    private List<RoomWall> growableWalls;
    private List<RoomWall> cornerSegments;
    private List<RoomWall> segments;
    private List<RoomWall> centerSegments;

    void Start()
    {
        tr = transform;
        cam = Camera.main;
        random = new System.Random(DateTime.Now.Millisecond);
        rooms = new List<Room>();
        growableWalls = new List<RoomWall>();
        cornerSegments = new List<RoomWall>();
        segments = new List<RoomWall>();
        centerSegments = new List<RoomWall>();
        texture = new Texture2D(testWalls.width, testWalls.height) { filterMode = FilterMode.Point };
        scanTexture = new Texture2D(transparent.width, transparent.height) { filterMode = FilterMode.Point };
        ResetTexture();
        ResetDebugTexture();
        RandomRooms();
        //InvokeRepeating("TakeScreenshot", 0, 0.1f);
    }

    void TakeScreenshot()
    {
        Application.CaptureScreenshot(screenshotCount + ".png");
        screenshotCount++;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            scanRenderer.enabled = !scanRenderer.enabled;
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ResetTexture();
            ResetDebugTexture();
        }
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            RandomWalls();
            RandomRooms();
            ResetDebugTexture();
        }
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
        {
            ResetTexture();
            RandomRooms();
            ResetDebugTexture();
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out selected) && selected.transform == tr)
            {
                var x = (int)(selected.textureCoord.x * texture.width);
                var y = (int)(selected.textureCoord.y * texture.height);

                if (!CheckRect(x - 1, y - 1, x + 1, y + 1, Color.white, Color.white)) return;

                var color = new Color(Random.value * 0.6f + 0.1f, Random.value * 0.6f + 0.1f,
                                      Random.value * 0.6f + 0.1f);
                rooms.Add(new Room(color,
                                   new List<GridVector>
                               {
                                   new GridVector(x - 1, y - 1),
                                   new GridVector(x - 1, y + 1),
                                   new GridVector(x + 1, y + 1),
                                   new GridVector(x + 1, y - 1)
                               }));
                DrawRoom(rooms[rooms.Count - 1]);
            }
        }

        ResetDebugTexture();
        foreach (var room in rooms)
        {
            foreach (var wall in room.walls)
            {
                BresenhamLine(wall, room.color);
            }
        }
        texture.Apply();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            paused = !paused;
        }
        if (!paused)
        {
            growableRooms = 0;
            growableCorners = 0;
            bigCorner = 0;
            bigCenter = 0;
            foreach (var room in rooms)
            {
                segments.Clear();
                growableWalls.Clear();
                cornerSegments.Clear();
                centerSegments.Clear();
                foreach (var wall in room.walls)
                {
                    segments.AddRange(FindSegments(wall, Color.white, room.color));
                    foreach (var segment in segments)
                    {
                        if ((segment.start == wall.start && segment.end == wall.end) ||
                            (segment.start == wall.end && segment.end == wall.start))
                        {
                            growableWalls.Add(segment);
                        }
                        else if (segment.start == wall.start || segment.end == wall.end ||
                                    segment.start == wall.end || segment.end == wall.start)
                        {
                            cornerSegments.Add(segment);
                            growableCorners++;
                        }
                        else
                        {
                            centerSegments.Add(segment);
                        }
                    }
                }

                if (cornerSegments.Count > 0)
                {
                    var l = LongWall(cornerSegments).length;
                    if (bigCorner < l && !room.canSpawn)
                    {
                        bigCornerRoom = room;
                        bigCorner = l;
                    }
                }
                if (centerSegments.Count > 0)
                {
                    var l = LongWall(centerSegments).length;
                    if (bigCenter < l)
                    {
                        bigCenterRoom = room;
                        bigCenter = l;
                    }
                }

                if (growableWalls.Count > 0)
                {
                    room.GrowWall(LongWall(growableWalls));
                    growableRooms++;
                }
                else if (growCorners)
                {
                    if (cornerSegments.Count > 0 && room.canSpawn)
                    {
                        room.GrowWall(LongWall(cornerSegments));
                        room.canSpawn = false;
                    }
                }
                else if (growCenters)
                {
                    if (cornerSegments.Count > 0)
                    {
                        room.GrowWall(LongWall(cornerSegments));
                    }
                    else if (centerSegments.Count > 0)
                    {
                        room.GrowWall(LongWall(centerSegments));
                    }
                }
                foreach (var wall in room.walls)
                {
                    BresenhamLine(wall, room.color);
                }
                texture.Apply();
                scanTexture.Apply();
            }
            if (growableRooms == 0)
            {
                if (growableCorners > 0)
                {
                    growCorners = true;
                    bigCornerRoom.canSpawn = true;
                }
                else
                {
                    growCorners = false;
                    growCenters = true;
                    bigCenterRoom.canSpawn = true;
                }
            }
        }
        
        renderer.material.mainTexture = texture;
        scanRenderer.material.mainTexture = scanTexture;
    }

    List<RoomWall> FindSegments(RoomWall wall, Color freeColor, Color roomColor)
    {
        var moved = wall + wall.outwards.minimized;
        var x0 = moved.start.x;
        var y0 = moved.start.y;
        var x1 = moved.end.x;
        var y1 = moved.end.y;
        var segments = new List<RoomWall>();
        GridVector start = null;
        GridVector end = null;

        bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
        if (steep)
        {
            Swap(ref x0, ref y0);
            Swap(ref x1, ref y1);
        }
        if (x0 > x1)
        {
            Swap(ref x0, ref x1);
            Swap(ref y0, ref y1);
        }
        for (int x = x0; x <= x1; x++)
        {
            for (int y = y0; y <= y1; y++)
            {
                int coordX = steep ? y : x;
                int coordY = steep ? x : y;
                Color color = texture.GetPixel(coordX, coordY);
                if (color != freeColor && color != roomColor)
                {
                    if (end != null && start != null)
                    {
                        var segment = new RoomWall(start, end);
                        segment -= wall.outwards.minimized;
                        segments.Add(segment);
                        start = null;
                        end = null;
                    }
                    scanTexture.SetPixel(coordX, coordY, Color.red);
                }
                else
                {
                    if (start == null)
                    {
                        start = new GridVector(coordX, coordY);
                    }
                    end = new GridVector(coordX, coordY);
                    scanTexture.SetPixel(coordX, coordY, Color.green);
                }
            }
        }
        if (end != null && start != null)
        {
            var segment = new RoomWall(start, end);
            segment -= wall.outwards.minimized;
            segments.Add(segment);
        }
        return segments;
    }

    bool CheckRect(int x0, int y0, int x1, int y1, Color freeColor, Color roomColor)
    {
        bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
        if (steep)
        {
            Swap(ref x0, ref y0);
            Swap(ref x1, ref y1);
        }
        if (x0 > x1)
        {
            Swap(ref x0, ref x1);
            Swap(ref y0, ref y1);
        }
        for (int x = x0; x <= x1; x++)
        {
            for (int y = y0; y <= y1; y++)
            {
                Color color = texture.GetPixel(steep ? y : x, steep ? x : y);
                if (color != freeColor && color != roomColor) return false;
            }
        }
        return true;
    }

    bool CheckRect(GridVector start, GridVector end, Color freeColor, Color roomColor)
    {
        return CheckRect(start.x, start.y, end.x, end.y, freeColor, roomColor);
    }

    bool CheckRect(RoomWall wall, Color freeColor, Color roomColor)
    {
        return CheckRect(wall.start, wall.end, freeColor, roomColor);
    }

    void Swap<T>(ref T lhs, ref T rhs)
    {
        T temp = lhs;
        lhs = rhs;
        rhs = temp;
    }

    void BresenhamLine(int x0, int y0, int x1, int y1, Color color, Texture2D tex = null)
    {
        if (tex == null) tex = texture;
        bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
        if (steep)
        {
            Swap(ref x0, ref y0);
            Swap(ref x1, ref y1);
        }
        if (x0 > x1)
        {
            Swap(ref x0, ref x1);
            Swap(ref y0, ref y1);
        }
        int dx = x1 - x0;
        int dy = Math.Abs(y1 - y0);
        int error = dx/2;
        int ystep = (y0 < y1) ? 1 : -1;
        int y = y0;
        for (int x = x0; x <= x1; x++)
        {
            tex.SetPixel(steep ? y : x, steep ? x : y, color);
            error -= dy;
            if (error < 0)
            {
                y += ystep;
                error += dx;
            }
        }
    }

    void BresenhamLine(GridVector start, GridVector end, Color color, Texture2D tex = null)
    {
        BresenhamLine(start.x, start.y, end.x, end.y, color, tex);
    }

    void BresenhamLine(RoomWall wall, Color color, Texture2D tex = null)
    {
        BresenhamLine(wall.start, wall.end, color, tex);
    }

    RoomWall LongWall(List<RoomWall> walls)
    {
        walls.Sort(RoomWall.CompareByLength);
        var longWalls = new List<RoomWall>();
        foreach (var line in walls)
        {
            if (line.length == walls[walls.Count - 1].length)
            {
                longWalls.Add(line);
            }
        }
        random = new System.Random(DateTime.Now.Millisecond);
        return longWalls[random.Next(longWalls.Count - 1)];
    }

    void ResetTexture()
    {
        rooms.Clear();
        growCorners = false;
        growCenters = false;
        screenshotCount = 0;
        texture.SetPixels(testWalls.GetPixels(0, 0, testWalls.width, testWalls.height));
        texture.Apply();
        renderer.material.mainTexture = texture;
    }
    void ResetDebugTexture()
    {
        scanTexture.SetPixels(transparent.GetPixels(0, 0, transparent.width, transparent.height));
        scanTexture.Apply();
        scanRenderer.material.mainTexture = scanTexture;
    }

    void DrawRoom(Room room)
    {
        foreach (var wall in room.walls)
        {
            BresenhamLine(wall, room.color);
        }
        texture.Apply();
    }

    void RandomWalls()
    {
        rooms.Clear();
        growCorners = false;
        growCenters = false;
        screenshotCount = 0;
        texture.SetPixels(transparent.GetPixels(0, 0, transparent.width, transparent.height));
        var r = new List<GridVector>();
        for (var i = 0; i < 3; i++)
        {
            r.Add(new GridVector(random.Next(16, texture.width / 3), random.Next(16, texture.height / 3)));
            r.Add(new GridVector(random.Next(texture.width / 3 * 2, texture.width - 16), random.Next(texture.height / 3 * 2, texture.height - 16)));
        }
        for (var i = 0; i < r.Count; i += 2)
        {
            for (var x = r[i].x; x < r[i+1].x; x++)
            {
                for (var y = r[i].y; y < r[i+1].y; y++)
                {
                    texture.SetPixel(x, y, Color.black);
                }
            }
        }
        for (var i = 0; i < r.Count; i += 2)
        {
            for (int x = r[i].x + 1; x < r[i + 1].x - 1; x++)
            {
                for (int y = r[i].y + 1; y < r[i + 1].y - 1; y++)
                {
                    texture.SetPixel(x, y, Color.white);
                }
            }
        }
        texture.Apply();
        renderer.material.mainTexture = texture;
    }

    void RandomRooms()
    {
        for (int i = 0; i < 10; i++)
        {
            int x = random.Next(0, texture.width);
            int y = random.Next(0, texture.height);
            if (CheckRect(x - 1, y - 1, x + 1, y + 1, Color.white, Color.white))
            {
                var color = new Color(Random.value*0.7f + 0.1f, Random.value*0.7f + 0.1f,
                                      Random.value*0.7f + 0.1f);
                rooms.Add(new Room(color,
                                   new List<GridVector>
                                       {
                                           new GridVector(x - 1, y - 1),
                                           new GridVector(x - 1, y + 1),
                                           new GridVector(x + 1, y + 1),
                                           new GridVector(x + 1, y - 1)
                                       }));
                DrawRoom(rooms[rooms.Count - 1]);
            }
        }
    }
}