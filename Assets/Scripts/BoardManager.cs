using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour
{
    public class CellData
    {
        public bool Passable;
        public CellObject ContainedObject;
    }

    [Header("Cài đặt Map")]
    public int Width;
    public int Height;
    public Tile[] GroundTiles;
    public Tile[] WallTiles;

    [Header("Cài đặt Prefabs (Bắt buộc kéo thả)")]
    public FoodObject[] FoodPrefab;
    public WallObject WallPrefab;
    public Enemy[] EnemyPrefab;

    private Tilemap m_Tilemap;
    private Grid m_Grid;
    private CellData[,] m_BoardData;
    private List<Vector2Int> m_EmptyCellsList;

    public int minFood = 3, maxFood = 8;
    public int minWall = 6, maxWall = 10;
    public int minEnemy = 1, maxEnemy = 3;

    public ExitCellObject ExitCellPrefab;
    public Camera cam;
    


    public void Init()
    {
        // 1. Tìm Tilemap và Grid
        m_Tilemap = GetComponentInChildren<Tilemap>();
        m_Grid = GetComponentInChildren<Grid>();

        // Kiểm tra lỗi nếu quên tạo Grid/Tilemap
        if (m_Tilemap == null || m_Grid == null)
        {
            Debug.LogError("LỖI: Không tìm thấy Grid hoặc Tilemap! Hãy đảm bảo GameObject 'Grid' nằm bên trong BoardManager.");
            return;
        }

        m_EmptyCellsList = new List<Vector2Int>();
        m_BoardData = new CellData[Width, Height];

        // 2. Tạo sàn và tường bao quanh
        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                Tile tile;
                m_BoardData[x, y] = new CellData();

                if (x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
                {
                    // Chọn ngẫu nhiên gạch tường (nếu có)
                    if (WallTiles != null && WallTiles.Length > 0)
                        tile = WallTiles[Random.Range(0, WallTiles.Length)];
                    else
                        tile = null;

                    m_BoardData[x, y].Passable = false;
                }
                else
                {
                    // Chọn ngẫu nhiên gạch sàn (nếu có)
                    if (GroundTiles != null && GroundTiles.Length > 0)
                        tile = GroundTiles[Random.Range(0, GroundTiles.Length)];
                    else
                        tile = null;

                    m_BoardData[x, y].Passable = true;
                    m_EmptyCellsList.Add(new Vector2Int(x, y));
                }

                m_Tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }

        // Loại bỏ vị trí (1,1) để tránh sinh vật cản ngay chỗ Player đứng lúc đầu
        m_EmptyCellsList.Remove(new Vector2Int(1, 1));

        Vector2Int endCoord = new Vector2Int(Width - 2, Height - 2);
        AddObject(Instantiate(ExitCellPrefab), endCoord);
        m_EmptyCellsList.Remove(endCoord);

        GameManager.Instance.PlayerController.Cell = new Vector2Int(1, 1);

        // 3. Sinh Tường và Thức ăn
        GenerateEnemy();
        GenerateWall();
        GenerateFood();
    }

    public Vector3 CellToWorld(Vector2Int cellIndex)
    {
        return m_Grid.GetCellCenterWorld((Vector3Int)cellIndex);
    }

    public CellData GetCellData(Vector2Int cellIndex)
    {
        if (cellIndex.x < 0 || cellIndex.x >= Width ||
            cellIndex.y < 0 || cellIndex.y >= Height)
            return null;

        return m_BoardData[cellIndex.x, cellIndex.y];
    }

    public Tile GetCellTile(Vector2Int cellIndex)
    {
        return m_Tilemap.GetTile<Tile>(new Vector3Int(cellIndex.x, cellIndex.y, 0));
    }

    public void SetCellTile(Vector2Int cellIndex, Tile tile)
    {
        m_Tilemap.SetTile(new Vector3Int(cellIndex.x, cellIndex.y, 0), tile);
    }

    void AddObject(CellObject obj, Vector2Int coord)
    {
        CellData data = m_BoardData[coord.x, coord.y];

        obj.Init(coord);
        obj.transform.position = CellToWorld(coord);

        data.ContainedObject = obj;
        // Dòng này cần file CellObject.cs đã sửa ở bước trước
        data.Passable = obj.PassableAfterPlaced;
    }
    void GenerateEnemy()
    {
        int enemyCount = Random.Range(minEnemy, maxEnemy);
        for (int i = 0; i < enemyCount; i++)
        {
            if (m_EmptyCellsList.Count == 0) break;
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];

            m_EmptyCellsList.RemoveAt(randomIndex);

            // --- KIỂM TRA QUAN TRỌNG ĐỂ TRÁNH LỖI INSTANTIATE NULL ---
            if (EnemyPrefab == null)
            {
                Debug.LogError("LỖI: Chưa kéo Enemy Prefab vào BoardManager!");
                return;
            }
            int chosenEnemy = (Random.value < 0.7f) ? 0 : 1;
            Enemy newEnemy = Instantiate(EnemyPrefab[chosenEnemy]);
            AddObject(newEnemy, coord);
        }
    }
    void GenerateWall()
    {
        int wallCount = Random.Range(minWall, maxWall);

        for (int i = 0; i < wallCount; ++i)
        {
            if (m_EmptyCellsList.Count == 0) break;

            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];

            m_EmptyCellsList.RemoveAt(randomIndex);

            // --- KIỂM TRA QUAN TRỌNG ĐỂ TRÁNH LỖI INSTANTIATE NULL ---
            if (WallPrefab == null)
            {
                Debug.LogError("LỖI: Chưa kéo Wall Prefab vào BoardManager!");
                return;
            }

            WallObject newWall = Instantiate(WallPrefab);
            AddObject(newWall, coord);
        }
    }

    void GenerateFood()
    {
        int foodCount = Random.Range(minFood, maxFood);

        for (int i = 0; i < foodCount; ++i)
        {
            if (m_EmptyCellsList.Count == 0) break;

            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];

            m_EmptyCellsList.RemoveAt(randomIndex);

            // --- KIỂM TRA QUAN TRỌNG ---
            if (FoodPrefab == null)
            {
                Debug.LogError("LỖI: Chưa kéo Food Prefab vào BoardManager!");
                return;
            }
            int chosenPrefab = (Random.value < 0.7f) ? 0 : 1;
            FoodObject newFood = Instantiate(FoodPrefab[chosenPrefab]);
            AddObject(newFood, coord);
        }
    }
    public void Clean()
    {
        //no board data, so exit early, nothing to clean
        if (m_BoardData == null)
            return;


        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                var cellData = m_BoardData[x, y];

                if (cellData.ContainedObject != null)
                {
                    //CAREFUL! Destroy the GameObject NOT just cellData.ContainedObject
                    //Otherwise what you are destroying is the JUST CellObject COMPONENT
                    //and not the whole gameobject with sprite
                    Destroy(cellData.ContainedObject.gameObject);
                }

                SetCellTile(new Vector2Int(x, y), null);
            }
        }
        if (GameManager.Instance.m_CurrentLevel % 10 == 0)
        {
            maxEnemy += 1;
            if (GameManager.Instance.m_CurrentLevel % 10 < 30)
            {
                minEnemy += 1;
                Width += 1;
                Height += 1;
                cam.orthographicSize += 1;
                minFood += 1;
                maxFood += 1;
                minWall += 2;
                maxWall += 2;
            }
        }
    }
}