using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using cakeslice;
#nullable enable

public struct Coordinates
{
    public Coordinates(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    public int x { get; set; }
    public int y { get; set; }

    public static bool operator ==(Coordinates a, Coordinates b)
    {
        return (a.x == b.x && a.y == b.y);
    }
    public static bool operator !=(Coordinates a, Coordinates b)
    {
        return !(a.x == b.x && a.y == b.y);
    }
}
public class Generator : MonoBehaviour
{
    public GameObject prefab;
    private Dictionary<CellType, Color> cellTypesColors = new Dictionary<CellType, Color>();

    private const float step50 = 0.05f;
    private const int grid_side = 300;

    //Хранит типы ячеек по координатам индекса
    private CellType[,] grid;
    private GameObject[,] prefab_grid;

    public const int vertical_offset = -25; //Смещение от нуля по горизонтали В процентах
    public const int width_of_the_river = 15;

    
    
    [Serializable]
    public struct CellTypeColors {
        public CellType cell_t;
        public Color color;
    }

    public CellTypeColors[] cellTypeColorsForInspector;
    private List<Coordinates> near_river_zone;

    private Dictionary<CellType, HashSet<Coordinates>> buildingMap = new Dictionary<CellType, HashSet<Coordinates>>();

    public int mult = 1;
    public enum CellType
    {
        empty,
        little_storage,
        big_storage,
        pumping_station,
        tank,
        electrical_substation,
        capitol,
        checkpoint,
        wall,
        river,
        near_river,
        wharf_dry,
        wharf_fluid,
    }


    private HashSet<Coordinates> first_zone         = new HashSet<Coordinates>();
    private HashSet<Coordinates> second_dry_zone    = new HashSet<Coordinates>();
    private HashSet<Coordinates> second_fluid_zone  = new HashSet<Coordinates>();
    private HashSet<Coordinates> third_zone         = new HashSet<Coordinates>();
    private HashSet<Coordinates> border_zone        = new HashSet<Coordinates>();
    private HashSet<Coordinates> outside_zone       = new HashSet<Coordinates>();

    public enum SizeOfPreset
    {
        _1x1,
        _1x2,
        _1x3,
        _2x2,
        _2x3,
        _2x4,
        _3x3,
        _3x4,
        _4x5,
        _6x5
    }
    [Serializable]
    public struct PresetInfo
    {
        public GameObject prefab;
        public SizeOfPreset size;
    }

    

    public PresetInfo[] first_zone_presets;
    public PresetInfo[] second_dry_zone_presets;
    public PresetInfo[] second_fluid_zone_presets;
    public PresetInfo[] third_zone_presets;
    public PresetInfo[] outside_zone_presets;

    public PresetInfo[] mandatory_presets_first_zones;
    public PresetInfo[] mandatory_presets_second_dry_zones;
    public PresetInfo[] mandatory_presets_second_fluid_zones;
    public PresetInfo[] mandatory_presets_third_zones;
    public PresetInfo[] mandatory_presets_outside_zones;

    private Dictionary<SizeOfPreset, List<GameObject>> _dict_first_zone_presets         = new Dictionary<SizeOfPreset, List<GameObject>>();
    private Dictionary<SizeOfPreset, List<GameObject>> _dict_second_dry_zone_presets    = new Dictionary<SizeOfPreset, List<GameObject>>();
    private Dictionary<SizeOfPreset, List<GameObject>> _dict_second_fluid_zone_presets  = new Dictionary<SizeOfPreset, List<GameObject>>();
    private Dictionary<SizeOfPreset, List<GameObject>> _dict_third_zone_presets         = new Dictionary<SizeOfPreset, List<GameObject>>();
    private Dictionary<SizeOfPreset, List<GameObject>> _dict_outside_zone_presets       = new Dictionary<SizeOfPreset, List<GameObject>>();

    public GameObject crane_prefab;
    public GameObject border_prefab;
    public GameObject kpp_prefab;

    public GameObject tube_prefab;
    public GameObject tube_box_prefab;

    public GameObject river_coast_45_prefab;
    public GameObject river_coast_45_grass_prefab;
    public GameObject river_top_coast_45_prefab;

    public GameObject riverQuantum;
    public GameObject coastLineQuantum;

    public GameObject border_corner;

    const int denominator_2x3 = 20;
    const int denominator_2x4 = 30;
    const int denominator_3x3 = 30;
    const int denominator_3x4 = 30;
    
    
    
    const int denominator_4x5 = 60;
    const int denominator_6x5 = 90;
    const int stop_factor = 10000;


    int top_side_coordinate = -grid_side, left_side_coordinate = 0, right_side_coordinate = 0;

    Coordinates xmin_ymin,
                xmin_ymax,
                xmax_ymax,
                xmax_ymin;

    CameraController cameraController;
    void Start()
    {
        cameraController = GameObject.FindGameObjectWithTag("Camera").GetComponentInChildren<CameraController>();

        foreach (CellTypeColors cell_t in cellTypeColorsForInspector)
        {
            cellTypesColors.Add(cell_t.cell_t, cell_t.color);
        }

        foreach (PresetInfo presetInfo in first_zone_presets)
        {
            if (!_dict_first_zone_presets.ContainsKey(presetInfo.size)) {
                _dict_first_zone_presets.Add(presetInfo.size, new List<GameObject>());
            }
            _dict_first_zone_presets[presetInfo.size].Add(presetInfo.prefab);
        }
        foreach (PresetInfo presetInfo in second_dry_zone_presets)
        {
            if (!_dict_second_dry_zone_presets.ContainsKey(presetInfo.size))
            {
                _dict_second_dry_zone_presets.Add(presetInfo.size, new List<GameObject>());
            }
            _dict_second_dry_zone_presets[presetInfo.size].Add(presetInfo.prefab);
        }
        foreach (PresetInfo presetInfo in second_fluid_zone_presets)
        {
            if (!_dict_second_fluid_zone_presets.ContainsKey(presetInfo.size))
            {
                _dict_second_fluid_zone_presets.Add(presetInfo.size, new List<GameObject>());
            }
            _dict_second_fluid_zone_presets[presetInfo.size].Add(presetInfo.prefab);
        }

        foreach (PresetInfo presetInfo in third_zone_presets)
        {
            if (!_dict_third_zone_presets.ContainsKey(presetInfo.size))
            {
                _dict_third_zone_presets.Add(presetInfo.size, new List<GameObject>());
            }
            _dict_third_zone_presets[presetInfo.size].Add(presetInfo.prefab);
        }

        foreach(PresetInfo presetInfo in outside_zone_presets)
        {
            if (!_dict_outside_zone_presets.ContainsKey(presetInfo.size))
            {
                _dict_outside_zone_presets.Add(presetInfo.size, new List<GameObject>());
            }
            _dict_outside_zone_presets[presetInfo.size].Add(presetInfo.prefab);
        }

        grid = new CellType[grid_side, grid_side];
        prefab_grid = new GameObject[grid_side, grid_side];
        near_river_zone = new List<Coordinates>();
        //Занулили карту изначально
        for (int i = 0; i < grid_side; i++)
        {
            for (int j = 0; j < grid_side; j++)
            {
                grid[i, j] = CellType.empty;
            }
        }

        StartCoroutine(building());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Color getCellTypeColor(CellType cell_t)
    {
        return cellTypesColors[cell_t];
    }

    private IEnumerator building() {

        generateRandomRiver();
        generateWharf();

        yield return generateClusters();
        buildTheBorder();
        //Исправить префабы береговых линий
        correctCoastLine();
        yield return generateOutsideCluster();
        //Дальнейшее заполнение административной зоны
        int size_thirdZone = third_zone.Count;
        while (true)
        {
            HashSet<Coordinates> _temp = new HashSet<Coordinates>();
            foreach (Coordinates coord in third_zone)
            {
                _temp.UnionWith(getNearEmtyCells(coord));
            }
            third_zone.UnionWith(_temp);

            yield return new WaitForSeconds(0.01f);
            DrawZone();

            if (size_thirdZone == third_zone.Count)
            {
                break;
            }
            size_thirdZone = third_zone.Count;
        }


        fillNodesOfGrapgh();
        

        foreach (var coord in first_zone)
        {
            createCell50(coord.x, coord.y, CellType.empty);
        }
        
        generateBuildingsOfTheZone(mandatory_presets_first_zones, _dict_first_zone_presets, Color.blue, first_zone);
        foreach (var coord in second_dry_zone)
        {
            createCell50(coord.x, coord.y, CellType.empty);
        }
        //generateMandatoruBuildingsOfTheZone(mandatory_presets_second_dry_zones, second_dry_zone);
        generateBuildingsOfTheZone(mandatory_presets_second_dry_zones, _dict_second_dry_zone_presets, Color.green, second_dry_zone);
        foreach (var coord in second_fluid_zone)
        {
            createCell50(coord.x, coord.y, CellType.empty);
        }
        //generateMandatoruBuildingsOfTheZone(mandatory_presets_second_fluid_zones, second_fluid_zone);
        generateBuildingsOfTheZone(mandatory_presets_second_fluid_zones, _dict_second_fluid_zone_presets, Color.cyan, second_fluid_zone, true);
        foreach (var coord in third_zone)
        {
            createCell50(coord.x, coord.y, CellType.empty);
        }
        //generateMandatoruBuildingsOfTheZone(mandatory_presets_third_zones, third_zone);
        generateBuildingsOfTheZone(mandatory_presets_third_zones, _dict_third_zone_presets, Color.red, third_zone);


        foreach (var coord in outside_zone)
        {
            createCell50(coord.x, coord.y, CellType.empty);
        }
        generateBuildingsOfTheZone(mandatory_presets_outside_zones, _dict_outside_zone_presets, Color.red, outside_zone);

        foreach (var item in buildingMap[CellType.wharf_dry])
        {
            var g_o = Instantiate(crane_prefab, new Vector3(item.x * step50, 0, item.y * step50), Quaternion.identity);
            /*createCell50(item.x, item.y - 1, CellType.empty);
            createCell50(item.x, item.y - 4, CellType.empty);
            createCell50(item.x, item.y - 2, CellType.empty);
            createCell50(item.x, item.y - 3, CellType.empty);*/
            g_o.transform.Rotate(new Vector3(-90, 0, 0));
        }

        if (buildingMap.ContainsKey(CellType.wharf_fluid))
        {
            foreach (var item in buildingMap[CellType.wharf_fluid])
            {
                var g_o = Instantiate(prefab, new Vector3(item.x * step50, 0, item.y * step50), Quaternion.identity);
                /*createCell50(item.x, item.y - 1, CellType.empty);
                createCell50(item.x, item.y - 4, CellType.empty);
                createCell50(item.x, item.y - 2, CellType.empty);
                createCell50(item.x, item.y - 3, CellType.empty);*/
                //g_o.transform.Rotate(new Vector3(-90, 0, 0));
            }
        }
        
        pathSearch();
        tubeBuilding();
    }

    

    private void generateRandomRiver() 
    {
        int rightPartdDirection = UnityEngine.Random.Range(-1, 2);
        int leftPartdDirection = UnityEngine.Random.Range(-1, 2);
        
        /*int leftPartdDirection = -1;
        int rightPartdDirection = -1;*/

        Coordinates centerPoint = new Coordinates(0, grid_side * vertical_offset / 100);

        Coordinates currentPoint = centerPoint;

        //Рисуем правую часть реки
        while (isCoordinateCorrect(currentPoint.x, currentPoint.y))
        {
            float? rotation = null;
            if (rightPartdDirection == 1)
            {
                rotation = 180;
            }
            if (rightPartdDirection == -1)
            {
                rotation = -90;
            }
            createCellNearRiver(currentPoint.x, currentPoint.y,  rotation);
            for (int i = 1; i <= width_of_the_river; i++)
            {
                int river_y_coord = currentPoint.y - i;
                if (river_y_coord < -grid_side / 2)
                {
                    break;
                }
                if (i == width_of_the_river)
                {
                    createCellNearRiver(currentPoint.x, river_y_coord, rotation - 180);
                }
                else
                {
                    createCell50(currentPoint.x, river_y_coord, CellType.river);
                }
            }
            //createCell50(currentPoint.x, currentPoint.y, CellType.near_river);
            near_river_zone.Add(currentPoint);
            currentPoint.y += rightPartdDirection;
            currentPoint.x += 1;
        }

        currentPoint = centerPoint;
        //int leftPartdDirection = UnityEngine.Random.Range(-1, 2);
        currentPoint.y += leftPartdDirection;
        currentPoint.x -= 1;
        
        //исправление ошибок в месте стыка двух половин рек
        if (rightPartdDirection == 0 && leftPartdDirection == 1)
        {
            currentPoint.y += leftPartdDirection - 2;
        }
        
        

        while (isCoordinateCorrect(currentPoint.x, currentPoint.y))
        {
            float? rotation = null;
            if (leftPartdDirection == 1)
            {
                rotation = -90;
            }
            if (leftPartdDirection == -1)
            {
                rotation = -180;
            }
            createCellNearRiver(currentPoint.x, currentPoint.y, rotation);

            for (int i = 1; i <= width_of_the_river; i++)
            {
                int river_y_coord = currentPoint.y - i;
                if (river_y_coord < -grid_side / 2)
                {
                    break;
                }
                if (i == width_of_the_river)
                {
                    createCellNearRiver(currentPoint.x, river_y_coord, rotation - 180);
                }
                else
                {
                    createCell50(currentPoint.x, river_y_coord, CellType.river);
                }
            }
            near_river_zone.Insert(0, currentPoint);
            currentPoint.y += leftPartdDirection;
            currentPoint.x -= 1;
        }

        //исправление ошибок в месте стыка двух половин рек
        if ((rightPartdDirection == -1 && leftPartdDirection == 0) || (rightPartdDirection == -1 && leftPartdDirection == -1))
        {
            createCellNearRiver(centerPoint.x, centerPoint.y);
            createCell50(centerPoint.x, centerPoint.y + 1, CellType.empty);
        }

        if (rightPartdDirection == 1 && leftPartdDirection == 1)
        {
            createCellNearRiver(centerPoint.x, centerPoint.y + 1);
            createCell50(centerPoint.x, centerPoint.y, CellType.empty);
            createCell50(centerPoint.x, centerPoint.y, CellType.river);
            createCellNearRiver(centerPoint.x, centerPoint.y - width_of_the_river);
        }


        //Bottom coastLine
        if ((rightPartdDirection == 1 && leftPartdDirection == 0))
        {            
            createCellNearRiver(centerPoint.x, centerPoint.y - width_of_the_river);
            createCell50(centerPoint.x, centerPoint.y - width_of_the_river - 1, CellType.empty);
        }

        if ((rightPartdDirection == 0 && leftPartdDirection == 1))
        {
            createCell50(centerPoint.x, centerPoint.y - width_of_the_river, CellType.empty);
            createCell50(centerPoint.x, centerPoint.y - width_of_the_river - 1, CellType.empty);
            createCell50(centerPoint.x - 1, centerPoint.y - width_of_the_river, CellType.empty);
            createCell50(centerPoint.x - 1, centerPoint.y - width_of_the_river - 1, CellType.empty);
            createCellNearRiver(centerPoint.x - 1, centerPoint.y - width_of_the_river);
            createCellNearRiver(centerPoint.x, centerPoint.y - width_of_the_river);
        }
        if ((rightPartdDirection == 1 && leftPartdDirection == 1))
        {
            createCell50(centerPoint.x, centerPoint.y - width_of_the_river - 1, CellType.empty);
        }

        if ((rightPartdDirection == -1 && leftPartdDirection == -1))
        {
            createCell50(centerPoint.x, centerPoint.y - width_of_the_river, CellType.empty);
            createCell50(centerPoint.x, centerPoint.y - width_of_the_river - 1, CellType.empty);
            createCellNearRiver(centerPoint.x, centerPoint.y - width_of_the_river - 1);
            createCell50(centerPoint.x, centerPoint.y - width_of_the_river, CellType.river);
        }

        if ((rightPartdDirection == 0 && leftPartdDirection == -1))
        {
            createCell50(centerPoint.x, centerPoint.y - width_of_the_river, CellType.empty);
            createCellNearRiver(centerPoint.x, centerPoint.y - width_of_the_river, rotation: 0);
        }
    }

    private void correctCoastLine()
    {
        foreach (var coord in near_river_zone)
        {
            if (xmin_ymin.x <= coord.x && coord.x <= xmax_ymin.x)
            {
                continue;
            }
            int n_x = coord.x + grid_side / 2;
            int n_y = coord.y + grid_side / 2 + 1;

            GameObject cell = prefab_grid[n_x, n_y];
            if (cell != null)
            {
                Quaternion rotation = cell.transform.rotation;
                Destroy(cell);
                prefab_grid[n_x, n_y] = null;

                cell = Instantiate(river_coast_45_grass_prefab, new Vector3(coord.x * step50, 0, (coord.y * step50) + 1 * step50), Quaternion.identity);
                cell.transform.rotation = rotation;
                prefab_grid[n_x, n_y] = cell;
            }
        }
    }

    private void createCellNearRiver(int x, int y, float? rotation = null)
    {
        int n_x = x + grid_side / 2;
        int n_y = y + grid_side / 2;

        grid[n_x, n_y] = CellType.near_river;

        GameObject cell = prefab_grid[n_x, n_y];

        if (cell == null)
        {
            if (rotation != null)
            {
                GameObject cell_top = prefab_grid[n_x, n_y + 1];
                

                cell = Instantiate(river_coast_45_prefab, new Vector3(x * step50, 0, y * step50), Quaternion.identity);

                cell.transform.Rotate(new Vector3(0, 1, 0), rotation.Value);
                cell.transform.Rotate(new Vector3(1, 0, 0), -90);
                prefab_grid[n_x, n_y] = cell;

                if (cell_top == null && isCoordinateCorrect(x, y + 1))
                {
                    grid[n_x, n_y + 1] = CellType.near_river;

                    cell_top = Instantiate(river_top_coast_45_prefab, new Vector3(x * step50, 0, (y * step50) + 1 * step50), Quaternion.identity);
                    cell_top.transform.Rotate(new Vector3(0, 1, 0), rotation.Value);
                    cell_top.transform.Rotate(new Vector3(1, 0, 0), -90);
                    prefab_grid[n_x, n_y + 1] = cell_top;

                    //prefab_grid[n_x, n_y + 1] = cell_top;
                }
                else if (cell_top != null && grid[n_x, n_y + 1] == CellType.river)
                {
                    if (n_y == 0)
                    {
                        return;
                    }
                    grid[n_x, n_y - 1] = CellType.near_river;

                    cell_top = Instantiate(river_coast_45_grass_prefab, new Vector3(x * step50, 0, (y * step50) - 1 * step50), Quaternion.identity);
                    cell_top.transform.Rotate(new Vector3(0, 1, 0), rotation.Value);
                    cell_top.transform.Rotate(new Vector3(1, 0, 0), -90);
                    prefab_grid[n_x, n_y - 1] = cell_top;
                }
                addBuildingToBuildingMap(CellType.near_river, new Coordinates(n_x, n_y));
                return;
            }
        }
        else
        {
            Destroy(cell);
            prefab_grid[n_x, n_y] = null;
        }

        cell = Instantiate(coastLineQuantum, new Vector3(x * step50, 0, y * step50), Quaternion.identity);
        cell.transform.Rotate(new Vector3(1, 0, 0), -90);
        prefab_grid[n_x, n_y] = cell;
        cell.GetComponent<MeshRenderer>().material.color = getCellTypeColor(CellType.near_river);

    }

    private void createCell50(int x, int y, CellType cell_t)
    {
        int n_x = x + grid_side / 2;
        int n_y = y + grid_side / 2;

        grid[n_x, n_y] = cell_t;

        GameObject cell = prefab_grid[n_x, n_y];

        if (cell == null)
        {
            if (cell_t == CellType.river)
            {
                cell = Instantiate(riverQuantum, new Vector3(x * step50, 0, y * step50), Quaternion.identity);
                cell.transform.Rotate(new Vector3(1, 0, 0), -90);
                prefab_grid[n_x, n_y] = cell;
                //cell.GetComponent<MeshRenderer>().material.color = getCellTypeColor(cell_t);
                return;
            }
            cell = Instantiate(prefab, new Vector3(x * step50, 0, y * step50), Quaternion.identity);
            prefab_grid[n_x, n_y] = cell;
            cell.GetComponent<MeshRenderer>().material.color = getCellTypeColor(cell_t);
        }

        if (cell_t == CellType.empty)
        {
            Destroy(cell);
            prefab_grid[n_x, n_y] = null;
        }

    }

    private void generateWharf()
    {
        int radius = 20;

        Coordinates leftCoordinates = getLeftBorderOfCircleWithRadius(radius);
        bool searchMode = true;

        int fluid_wharf_count = UnityEngine.Random.Range(0, 3);
        int fluid_wharf_count_right = UnityEngine.Random.Range(0, 3);
        /*int fluid_wharf_count = 2;
        int fluid_wharf_count_right = 2;*/
        int dry_wharf_count = UnityEngine.Random.Range(1, 5);
        int wharf_distance = 7;
        int distance_to_next_wharf = wharf_distance;

        foreach (Coordinates coord in near_river_zone)
        {
            if (searchMode)
            {
                if (coord == leftCoordinates)
                {
                    
                    searchMode = false;
                    if (fluid_wharf_count > 0)
                    {
                        createCell50(coord.x, coord.y, CellType.wharf_fluid);
                        fluid_wharf_count--;

                        addBuildingToBuildingMap(CellType.wharf_fluid, coord);
                    }
                    else 
                    {
                        createCell50(coord.x, coord.y, CellType.wharf_dry);
                        dry_wharf_count--;

                        addBuildingToBuildingMap(CellType.wharf_dry, coord);
                    }
                   
                    distance_to_next_wharf = wharf_distance;
                }
            }
            else
            {
                if (distance_to_next_wharf == 0)
                {
                    if (fluid_wharf_count > 0)
                    {
                        createCell50(coord.x, coord.y, CellType.wharf_fluid);
                        fluid_wharf_count--;

                        addBuildingToBuildingMap(CellType.wharf_fluid, coord);
                    }
                    else if (dry_wharf_count > 0)
                    {
                        createCell50(coord.x, coord.y, CellType.wharf_dry);
                        dry_wharf_count--;

                        addBuildingToBuildingMap(CellType.wharf_dry, coord);
                    }
                    else if (fluid_wharf_count_right > 0)
                    {
                        createCell50(coord.x, coord.y, CellType.wharf_fluid);
                        fluid_wharf_count_right--;

                        addBuildingToBuildingMap(CellType.wharf_fluid, coord);
                    }
                    else
                    {
                        break;
                    }
                    distance_to_next_wharf = wharf_distance;
                }
                else
                {
                    distance_to_next_wharf--;
                }
            }
        }

    }

    private Coordinates getLeftBorderOfCircleWithRadius(int radius)
    {

        foreach (Coordinates coord in near_river_zone)
        {
            float s = coord.x * coord.x + (-vertical_offset * grid_side / 100 + coord.y) * (-vertical_offset * grid_side / 100 + coord.y);
            if (s < radius * radius)
            {
                return coord;
            }
        }
        throw new InvalidOperationException("getLeftBorderOfCircleWithRadius: There is no intersections");

    }


    

    private CellType getCellFromGrid(int x, int y) {
        int n_x = x + grid_side / 2;
        int n_y = y + grid_side / 2;

        return grid[n_x, n_y];
    }


    private void addBuildingToBuildingMap(CellType cell_t, Coordinates coord)
    {
        if (!buildingMap.ContainsKey(cell_t))
        {
            buildingMap[cell_t] = new HashSet<Coordinates>();
        }
        buildingMap[cell_t].Add(coord);
    }

    private IEnumerator generateClusters()
    {
        //Первый этап генерации кластеров - заполнение зон

        foreach (Coordinates coord in buildingMap[CellType.wharf_dry])
        {
            first_zone.UnionWith(getNearEmtyCells(coord));
        }

        if (buildingMap.ContainsKey(CellType.wharf_fluid))
        {
            foreach (Coordinates coord in buildingMap[CellType.wharf_fluid])
            {
                second_fluid_zone.UnionWith(getNearEmtyCells(coord));
            }
        }
        
        DrawZone();
        yield return new WaitForSeconds(0.01f);

        //Второй этап - генерация первой и вторых зон
        //int mult = 3;
        int count_of_iterations_first_zone = 1 * mult;
        int count_of_iterations_second_fluid_zone = 2 * mult;
        int count_of_iterations_second_dry_zone = 3 * mult;
        int count_of_iterations_third_zone = 2 * mult;

        while (count_of_iterations_first_zone > 0 || count_of_iterations_second_dry_zone > 0 || count_of_iterations_second_fluid_zone > 0) { 
            if (count_of_iterations_first_zone > 0)
            {
                HashSet<Coordinates> _temp = new HashSet<Coordinates>();
                foreach (Coordinates coord in first_zone)
                {
                    _temp.UnionWith(getNearEmtyCells(coord));
                }
                first_zone.UnionWith(_temp);
                count_of_iterations_first_zone--;
            }
            if (count_of_iterations_first_zone == 0 && count_of_iterations_second_dry_zone != 0) {
                HashSet<Coordinates> _temp = new HashSet<Coordinates>();
                foreach (Coordinates coord in first_zone)
                {
                    _temp.UnionWith(getNearEmtyCells(coord));
                }
                second_dry_zone.UnionWith(_temp);
            }

            if (count_of_iterations_second_dry_zone > 0 && count_of_iterations_first_zone == 0)
            {
                HashSet<Coordinates> _temp = new HashSet<Coordinates>();
                foreach (Coordinates coord in second_dry_zone)
                {
                    _temp.UnionWith(getNearEmtyCells(coord));
                }
                second_dry_zone.UnionWith(_temp);
                count_of_iterations_second_dry_zone--;
            }
            if (count_of_iterations_second_fluid_zone > 0)
            {
                HashSet<Coordinates> _temp = new HashSet<Coordinates>();
                foreach (Coordinates coord in second_fluid_zone)
                {
                    _temp.UnionWith(getNearEmtyCells(coord));
                }
                second_fluid_zone.UnionWith(_temp);
                count_of_iterations_second_fluid_zone--;
            }
            yield return new WaitForSeconds(0.01f);
            DrawZone();
        }

        //Третий этап - генерация третьей зоны
        if (second_dry_zone.Count != 0)
        {
            //Найдем точки кристализации
            HashSet<Coordinates> __temp = new HashSet<Coordinates>();
            foreach (Coordinates coord in second_dry_zone)
            {
                __temp.UnionWith(getNearEmtyCells(coord));
            }

            Coordinates[] arr = new Coordinates[__temp.Count];
            __temp.CopyTo(arr);

            var center_of_third_zone = arr[UnityEngine.Random.Range(0, __temp.Count)];


            // TODO: Найти наиболее удаленную точку от жикостной зоны
            third_zone.Add(center_of_third_zone);

            while (count_of_iterations_third_zone > 0)
            {
                HashSet<Coordinates> _temp = new HashSet<Coordinates>();
                foreach (Coordinates coord in third_zone)
                {
                    _temp.UnionWith(getNearEmtyCells(coord));
                }
                third_zone.UnionWith(_temp);
                count_of_iterations_third_zone--;

                yield return new WaitForSeconds(0.01f);
                DrawZone();
            }
        }
        

        //Узнаем границы территории порта
        findCoordinatesForBorder(first_zone);
        findCoordinatesForBorder(second_dry_zone);
        findCoordinatesForBorder(second_fluid_zone);
        findCoordinatesForBorder(third_zone);

        right_side_coordinate++;
        left_side_coordinate--;
        top_side_coordinate += 2;  

        //cameraController.setCameraLimits(leftLimit: camera_left_limit, rightLimit: camera_right_limit, topLimit: camera_top_limit, bottomLimit: camera_bottom_limit);
        yield return new WaitForSeconds(0.01f);
    }

    private IEnumerator generateOutsideCluster()
    {
        int count_of_iterations_outside_zone = 20 * mult;

        HashSet<Coordinates> critic_points = new HashSet<Coordinates>();
        critic_points.Add(new Coordinates(xmin_ymax.x + 1, xmin_ymax.y + 1));
        critic_points.Add(new Coordinates(xmax_ymax.x + 1, xmax_ymax.y + 1));
        foreach (Coordinates coord in critic_points)
        {
            outside_zone.UnionWith(getNearEmtyCells(coord));
        }
        DrawZone();
        //yield return new WaitForSeconds(0.01f);

        while (count_of_iterations_outside_zone > 0)
        {
            HashSet<Coordinates> _temp = new HashSet<Coordinates>();

            foreach (Coordinates coord in outside_zone)
            {
                _temp.UnionWith(getNearEmtyCells(coord));
            }
            outside_zone.UnionWith(_temp);
            count_of_iterations_outside_zone--;
        }


        yield return new WaitForSeconds(0.01f);
        DrawZone();
    }


    private void findCoordinatesForBorder(HashSet<Coordinates> zone) 
    {
        foreach (var coord in zone)
        {
            if (coord.x > right_side_coordinate)
            {
                right_side_coordinate = coord.x;
            }
            if (coord.x < left_side_coordinate)
            {
                left_side_coordinate = coord.x;
            }
            if (coord.y > top_side_coordinate)
            {
                top_side_coordinate = coord.y;
            }
        }
    }

    private void buildTheBorder() 
    {
        int x = 0, y = 0; 
        foreach (var coord in near_river_zone) 
        { 
            if (coord.x == left_side_coordinate)
            {
                x = coord.x;
                y = coord.y + 1;

                xmin_ymin = new Coordinates(x, y + 1);
                break;
            }
        }
        
        //Левая граница
        while (true)
        {
            if (y == top_side_coordinate)
            {
                xmin_ymax = new Coordinates(x, y);
                var corner = Instantiate(border_corner, new Vector3(x * step50, border_corner.transform.position.y, y * step50), Quaternion.identity);
                corner.transform.Rotate(new Vector3(-90, 0, -90));
                border_zone.Add(new Coordinates(x, y));
                break;
            }
            var g_o = Instantiate(border_prefab, new Vector3(x * step50, border_prefab.transform.position.y, y * step50), Quaternion.identity);
            border_zone.Add(new Coordinates(x, y));
            g_o.transform.Rotate(new Vector3(-90, 90, 0));
            
            y++;
        }
        //Верхняя граница
        var kpp_x_coord = (right_side_coordinate - left_side_coordinate) / 2 + left_side_coordinate;
        while (true)
        {
            if (x == xmin_ymax.x)
            {
                x++;
                continue;
            }
            GameObject g_o;
            if (x == kpp_x_coord)
            {
                g_o = Instantiate(kpp_prefab, new Vector3(x * step50, border_prefab.transform.position.y, y * step50), Quaternion.Euler(new Vector3(0, 90, 0)));
                border_zone.Add(new Coordinates(x, y));
            }
            else
            {
                if (x == right_side_coordinate)
                {
                    xmax_ymax = new Coordinates(x, y);
                    var corner = Instantiate(border_corner, new Vector3(x * step50, border_corner.transform.position.y, y * step50), Quaternion.identity);
                    corner.transform.Rotate(new Vector3(-90, 0, 0));
                    border_zone.Add(new Coordinates(x, y));
                    break;
                }
                g_o = Instantiate(border_prefab, new Vector3(x * step50, border_prefab.transform.position.y, y * step50), Quaternion.identity);
                border_zone.Add(new Coordinates(x, y));
            }

            g_o.transform.Rotate(new Vector3(-90, 0, 0));
            
            x++;
        }
        //Правая граница
        while (true)
        {
            if (y == xmax_ymax.y)
            {
                y--;
                continue;
            }
            var g_o = Instantiate(border_prefab, new Vector3(x * step50, border_prefab.transform.position.y, y * step50), Quaternion.identity);
            border_zone.Add(new Coordinates(x, y));
            g_o.transform.Rotate(new Vector3(-90, 90, 0));
            if (grid[x + grid_side / 2, y + grid_side / 2 - 1] == CellType.near_river)
            {
                xmax_ymin = new Coordinates(x, y);
                break;
            }
            y--;
        }

        

        cameraController.setCameraLimits(xmin_ymin: (xmin_ymin.x * step50, xmin_ymin.y * step50), xmin_ymax: (xmin_ymax.x * step50, xmin_ymax.y * step50),
                                         xmax_ymax: (xmax_ymax.x * step50, xmax_ymax.y * step50), xmax_ymin: (xmax_ymin.x * step50, xmax_ymin.y * step50));
        cameraController.setCameraStartPosition();
    }

    struct Building
    {
        public GameObject prefab;
         public  SizeOfPreset size;

        public Building(GameObject prefab, SizeOfPreset size) 
        {
            this.prefab = prefab;
            this.size = size;
        }
    };
    private void generateBuildingsOfTheZone(PresetInfo[] mandatory_buildings_of_the_zone,
                                            Dictionary<SizeOfPreset,
                                            List<GameObject>> buildings_of_the_zone,
                                            Color building_color,
                                            HashSet<Coordinates> zone,
                                            bool updateGrapgh = false)
    {
        if (zone.Count == 0)
        {
            return;
        }

        HashSet<Coordinates> busy_coordinates = new HashSet<Coordinates>();
        if (mandatory_buildings_of_the_zone.Length != 0)
        {
            foreach (var elem in mandatory_buildings_of_the_zone)
            {
                placeTheBuilding(elem.prefab, elem.size, stop_factor, zone, busy_coordinates, Color.red);
            }
        }
        

        List<Building> building_queue = new List<Building>();
        var count_of_cells_in_zone = zone.Count;
        for (int i = 0; i < count_of_cells_in_zone / denominator_6x5; i++)
        {
            var building = getPrefabForZoneOfSize(buildings_of_the_zone, SizeOfPreset._6x5);
            if (building != null)
            {
                building_queue.Add(new Building(building, SizeOfPreset._6x5));
            }

        }

        for (int i = 0; i < count_of_cells_in_zone / denominator_4x5; i++)
        {
            var building = getPrefabForZoneOfSize(buildings_of_the_zone, SizeOfPreset._4x5);
            if (building != null)
            {
                building_queue.Add(new Building(building, SizeOfPreset._4x5));
            }

        }

        for (int i = 0; i < count_of_cells_in_zone / denominator_3x4; i++)
        {
            var building = getPrefabForZoneOfSize(buildings_of_the_zone, SizeOfPreset._3x4);
            if (building != null)
            {
                building_queue.Add(new Building(building, SizeOfPreset._3x4));
            }

        }

        for (int i = 0; i < count_of_cells_in_zone / denominator_3x3; i ++)
        {
            var building = getPrefabForZoneOfSize(buildings_of_the_zone, SizeOfPreset._3x3);
            if (building != null)
            {
                building_queue.Add(new Building(building, SizeOfPreset._3x3));
            }
            
        }
        for (int i = 0; i < count_of_cells_in_zone / denominator_2x4; i++)
        {

            var building = getPrefabForZoneOfSize(buildings_of_the_zone, SizeOfPreset._2x4);
            if (building != null)
            {
                building_queue.Add(new Building(building, SizeOfPreset._2x4));
            }
            
        }
        for (int i = 0; i < count_of_cells_in_zone / denominator_2x3; i++)
        {
            var building = getPrefabForZoneOfSize(buildings_of_the_zone, SizeOfPreset._2x3);
            if (building != null)
            {
                building_queue.Add(new Building(building, SizeOfPreset._2x3));
            }
           // building_queue.Add(new Building(getPrefabForZoneOfSize(buildings_of_the_zone, SizeOfPreset._2x3), SizeOfPreset._2x3));
        }

        SizeOfPreset[] other_building_sizes = { SizeOfPreset._2x2, SizeOfPreset._1x3, SizeOfPreset._1x2, SizeOfPreset._1x1 };

        

        foreach (Building elem_in_queue in building_queue)
        {
            placeTheBuilding(elem_in_queue.prefab, elem_in_queue.size, stop_factor, zone, busy_coordinates, building_color, updateGrapgh);
        }

        while(true)
        {
            var rnd_ind = UnityEngine.Random.Range(0, other_building_sizes.Length);
            var building_size = other_building_sizes[rnd_ind];
            var building = getPrefabForZoneOfSize(buildings_of_the_zone, building_size);
            if (building != null)
            {
                if (!placeTheBuilding(building, building_size, stop_factor, zone, busy_coordinates, building_color, updateGrapgh))
                {
                    break;
                }
            }
        }
        //Заполняем оставшиеся пустые места
        while (true)
        {
            //var rnd_ind = UnityEngine.Random.Range(0, other_building_sizes.Length);
            var building_size = SizeOfPreset._1x1;
            var building = getPrefabForZoneOfSize(buildings_of_the_zone, building_size);
            if (building != null)
            {
                if (!placeTheBuilding(building, building_size, stop_factor, zone, busy_coordinates, building_color, updateGrapgh))
                {
                    break;
                }
            }
        }

        
    }

    private HashSet<Coordinates> getNearEmtyCells(Coordinates coord) {

        HashSet<Coordinates> returnValue = new HashSet<Coordinates>();
        //Проверяем путсые ячейки, которые не отмечены как потенциальная зона
        if (isCoordinateCorrect(coord.x + 1, coord.y) && (getCellFromGrid(coord.x + 1, coord.y) == CellType.empty) && !isAlreadyZoneCell(coord.x + 1, coord.y))
        {
            returnValue.Add(new Coordinates(coord.x + 1, coord.y));
        }
        if (isCoordinateCorrect(coord.x - 1, coord.y) && getCellFromGrid(coord.x - 1, coord.y) == CellType.empty && !isAlreadyZoneCell(coord.x - 1, coord.y))
        {
            returnValue.Add(new Coordinates(coord.x - 1, coord.y));
        }
        if (isCoordinateCorrect(coord.x, coord.y + 1) && getCellFromGrid(coord.x, coord.y + 1) == CellType.empty && !isAlreadyZoneCell(coord.x, coord.y + 1))
        {
            returnValue.Add(new Coordinates(coord.x, coord.y + 1));
        }
        if (isCoordinateCorrect(coord.x, coord.y - 1) && getCellFromGrid(coord.x, coord.y -1) == CellType.empty && !isAlreadyZoneCell(coord.x, coord.y - 1))
        {
            returnValue.Add(new Coordinates(coord.x, coord.y - 1));
        }



        if (isCoordinateCorrect(coord.x + 1, coord.y - 1) && getCellFromGrid(coord.x + 1, coord.y - 1) == CellType.empty && !isAlreadyZoneCell(coord.x + 1, coord.y - 1))
        {
            returnValue.Add(new Coordinates(coord.x + 1, coord.y - 1));
        }
        if (isCoordinateCorrect(coord.x - 1, coord.y + 1) && getCellFromGrid(coord.x - 1, coord.y + 1) == CellType.empty && !isAlreadyZoneCell(coord.x - 1, coord.y + 1))
        {
            returnValue.Add(new Coordinates(coord.x - 1, coord.y + 1));
        }
        if (isCoordinateCorrect(coord.x + 1, coord.y + 1) && getCellFromGrid(coord.x + 1, coord.y + 1) == CellType.empty && !isAlreadyZoneCell(coord.x + 1, coord.y + 1))
        {
            returnValue.Add(new Coordinates(coord.x + 1, coord.y + 1));
        }
        if (isCoordinateCorrect(coord.x - 1, coord.y - 1) && getCellFromGrid(coord.x - 1, coord.y - 1) == CellType.empty && !isAlreadyZoneCell(coord.x - 1, coord.y - 1))
        {
            returnValue.Add(new Coordinates(coord.x - 1, coord.y - 1));
        }

        return returnValue;
    }

    private bool isCoordinateCorrect(int x, int y) {
        return (x >= -grid_side / 2 && x < grid_side / 2 && y >= -grid_side / 2 && y < grid_side / 2);
    }

    private bool isAlreadyZoneCell(int x, int y) {
        var coord = new Coordinates(x, y);
        return (first_zone.Contains(coord) || second_dry_zone.Contains(coord) ||
                second_fluid_zone.Contains(coord) || third_zone.Contains(coord) || border_zone.Contains(coord));
    }

    private void DrawZone()
    {
        foreach (Coordinates coord in first_zone)
        {
            createCell50(coord.x, coord.y, CellType.little_storage);
        }
        foreach (Coordinates coord in second_dry_zone)
        {
            createCell50(coord.x, coord.y, CellType.big_storage);
        }
        foreach (Coordinates coord in second_fluid_zone)
        {
            createCell50(coord.x, coord.y, CellType.tank);
        }
        foreach (Coordinates coord in third_zone)
        {
            createCell50(coord.x, coord.y, CellType.capitol);
        }
        foreach (Coordinates coord in outside_zone)
        {
            createCell50(coord.x, coord.y, CellType.electrical_substation);
        }
    }

    private void shuffle(Coordinates[] arr) {

        for (int i = 0; i < 2 * arr.Length; i++)
        {
            int a = UnityEngine.Random.Range(0, arr.Length);
            int b = UnityEngine.Random.Range(0, arr.Length);

            Coordinates temp = arr[a];
            arr[a] = arr[b];
            arr[b] = temp;
        }
    }
    
    private GameObject? getPrefabForZoneOfSize(Dictionary<SizeOfPreset, List<GameObject>> zone, SizeOfPreset size_of_preset)
    {
        if (!zone.ContainsKey(size_of_preset))
        {
            return null;
        }
        var length_of_list = zone[size_of_preset].Count;
        var rnd_ind = UnityEngine.Random.Range(0, length_of_list);
        return zone[size_of_preset][rnd_ind];

    }

    private bool placeTheBuilding(GameObject building, SizeOfPreset size_of_building, int stop_factor, HashSet<Coordinates> hs_of_coordinates_in_zone, HashSet<Coordinates> busyCells, Color color, bool updateGraph = false)
    {
        Coordinates[] arr_of_coordinates_in_zone = new Coordinates[hs_of_coordinates_in_zone.Count];
        hs_of_coordinates_in_zone.CopyTo(arr_of_coordinates_in_zone);
        //Цикл по пыткам разместить здание в случайной точке
        for (int i = 0; i < stop_factor; i++) {
           
            var rnd_coord = arr_of_coordinates_in_zone[UnityEngine.Random.Range(0, arr_of_coordinates_in_zone.Length)];

            var buildingSize = convertBuildingSize(size_of_building);
            bool succeed = true;
            //Цикл по пыткам поворота здания при размещении
            for (int angle = 0; angle < 4; angle++)
            {
                succeed = true;
                //Проверка каждой точки здания свободна ли она для размещения 
                for (int x = 0; Math.Abs(x) < Math.Abs(buildingSize.x); x += Math.Sign(buildingSize.x))
                {
                    for (int y = 0; Math.Abs(y) < Math.Abs(buildingSize.y); y += Math.Sign(buildingSize.y))
                    {
                        var x_cell = rnd_coord.x + x;
                        var y_cell = rnd_coord.y + y;

                        if (!isCoordinateCorrect(x_cell, y_cell))
                        {
                            succeed = false;
                            break;
                        }
                        if (!hs_of_coordinates_in_zone.Contains(new Coordinates(x_cell, y_cell)))
                        {
                            succeed = false;
                            break;
                        }
                        if (busyCells.Contains(new Coordinates(x_cell, y_cell)))
                        {
                            succeed = false;
                            break;
                        }
                    }
                    if (!succeed)
                    {
                        break;
                    }
                }
                if (!succeed)
                {
                    buildingSize = rotateBuilding(buildingSize);
                }
                else
                {
                    //Разместить здание
                    for (int x = 0; Math.Abs(x) < Math.Abs(buildingSize.x); x += Math.Sign(buildingSize.x))
                    {
                        for (int y = 0; Math.Abs(y) < Math.Abs(buildingSize.y); y += Math.Sign(buildingSize.y))
                        {
                            var x_cell = rnd_coord.x + x;
                            var y_cell = rnd_coord.y + y;
                            busyCells.Add(new Coordinates(x_cell, y_cell));
                        }
                    }

                    var placedBuilding = Instantiate(building, new Vector3(rnd_coord.x * step50, 0, rnd_coord.y * step50), Quaternion.identity);
                    placedBuilding.transform.Rotate(new Vector3(-90, 0, 0));
                    placedBuilding.transform.RotateAround(placedBuilding.transform.position, Vector3.up, 90 * angle);

                    Transform transform = placedBuilding.transform;
                    if (updateGraph)
                    {
                        fillInfoAboutDisabledEdgesOfGraph(buildingSize, rnd_coord);

                        //Добавить поправочку для типа зданий
                        if (Math.Abs(buildingSize.x) * Math.Abs(buildingSize.y) > 4)
                        {
                            fillDistanationPoints(buildingSize, rnd_coord);
                        }
                    }
                    
                    //Если инстанциируем квартал - ничего не еделать, сделаем в скрипте квартала

                    if (placedBuilding.GetComponent<QuarterContent>())
                    {
                        placedBuilding.GetComponent<QuarterContent>().makeChildObjectsSelectable();
                        break;
                    }
                    //Если отдельный префаб, то сделать его выделяемым
                    var collider = placedBuilding.AddComponent<BoxCollider>();
                    collider.size = new Vector3(collider.size.x * 0.9f, collider.size.y * 0.9f, collider.size.z * 0.9f);
                    placedBuilding.layer = 3;
                    var outline_c = placedBuilding.AddComponent<Outline>();
                    outline_c.enabled = false;

                    var rg = placedBuilding.AddComponent<Rigidbody>();
                    rg.angularDrag = 100;
                    rg.mass = 100;
                    rg.drag = 100;
                    rg.transform.position = transform.position;
                    rg.transform.rotation = transform.rotation;
                    break;
                }
            }
            if (succeed)
            {
                return true;
            }
            
        }
        return false;
    }

    private Vector2Int convertBuildingSize(SizeOfPreset size) { 
        switch (size)
        {
            case SizeOfPreset._1x1:
                {
                    return new Vector2Int(-1, -1);
                }
            case SizeOfPreset._1x2:
                {
                    return new Vector2Int(-1, -2);
                }
            case SizeOfPreset._1x3:
                {
                    return new Vector2Int(-1, -3);
                }
            case SizeOfPreset._2x2:
                {
                    return new Vector2Int(-2, -2);
                }
            case SizeOfPreset._2x3:
                {
                    return new Vector2Int(-2, -3);
                }
            case SizeOfPreset._2x4:
                {
                    return new Vector2Int(-2, -4);
                }
            case SizeOfPreset._3x3:
                {
                    return new Vector2Int(-3, -3);
                }
            case SizeOfPreset._3x4:
                {
                    return new Vector2Int(-3, -4);
                }
            case SizeOfPreset._4x5:
                {
                    return new Vector2Int(-4, -5);
                }
            case SizeOfPreset._6x5:
                {
                    return new Vector2Int(-6, -5);
                }
            default:
                {
                    throw new InvalidOperationException("convertBuildingSize:DefaultValue Err");
                }
        }
    }


    //Выяснено научным путем
    private Vector2Int rotateBuilding(Vector2Int size) 
    {
        return new Vector2Int(size.y, -size.x);
    }










    //////////////////////////////
    ///Генерация труб
    ///

    private HashSet<Coordinates> nodes_of_grapgh = new HashSet<Coordinates>();
    //Информация в какие точки нельзя попасть из точки ключа
    private Dictionary<Coordinates, HashSet<Coordinates>> disabled_edges = new Dictionary<Coordinates, HashSet<Coordinates>>();

    private Dictionary<Coordinates, List<Coordinates>> path_to_point = new Dictionary<Coordinates, List<Coordinates>>();
    private HashSet<Coordinates> checked_points = new HashSet<Coordinates>();

    private List<Coordinates> distanation_points = new List<Coordinates>();

    private void fillNodesOfGrapgh() { 
    
        foreach (var coord in second_fluid_zone)
        {
            nodes_of_grapgh.Add(new Coordinates(coord.x, coord.y));
            nodes_of_grapgh.Add(new Coordinates(coord.x+1, coord.y));
            nodes_of_grapgh.Add(new Coordinates(coord.x, coord.y+1));
            nodes_of_grapgh.Add(new Coordinates(coord.x+1, coord.y+1));
        }

    }

    private void fillInfoAboutDisabledEdgesOfGraph(Vector2Int buildingSize, Coordinates base_cell_coord)
    {
        if (disabled_edges.Count == 0)
        {
            foreach (var coord in nodes_of_grapgh)
            {
                disabled_edges.Add(coord, new HashSet<Coordinates>());
            }
        }

        for (int x = 0; Math.Abs(x) < Math.Abs(buildingSize.x); x += Math.Sign(buildingSize.x))
        {
            for (int y = 0; Math.Abs(y) < Math.Abs(buildingSize.y); y += Math.Sign(buildingSize.y))
            {

                var x_cell = base_cell_coord.x + x;
                var y_cell = base_cell_coord.y + y;
                if (Math.Abs(x) < Math.Abs(buildingSize.x) - 1)
                {
                    if (Math.Sign(buildingSize.x) > 0)
                    {
                        var a = new Coordinates(x_cell + 1, y_cell + 1);
                        var b = new Coordinates(x_cell + 1, y_cell);

                        disabled_edges[a].Add(b);
                        disabled_edges[b].Add(a);
                    }
                    else
                    {
                        var a = new Coordinates(x_cell, y_cell);
                        var b = new Coordinates(x_cell, y_cell+1);

                        disabled_edges[a].Add(b);
                        disabled_edges[b].Add(a);
                    }
                }

                if ((Math.Abs(y) < Math.Abs(buildingSize.y) - 1)) 
                {
                    if (Math.Sign(buildingSize.y) > 0)
                    {
                        var a = new Coordinates(x_cell, y_cell + 1);
                        var b = new Coordinates(x_cell + 1, y_cell + 1);

                        disabled_edges[a].Add(b);
                        disabled_edges[b].Add(a);
                    }
                    else
                    {
                        var a = new Coordinates(x_cell, y_cell);
                        var b = new Coordinates(x_cell + 1, y_cell);

                        disabled_edges[a].Add(b);
                        disabled_edges[b].Add(a);
                    }
                }
            }
        }

    }

    private void pathSearch() { 

        if (!buildingMap.ContainsKey(CellType.wharf_fluid))
        {
            return;
        }

        foreach (var coord in buildingMap[CellType.wharf_fluid])
        {
            var start_point = new Coordinates(coord.x, coord.y + 1);
            var new_list = new List<Coordinates>();
            new_list.Add(start_point);
            path_to_point.Add(start_point, new_list);
        }
       

        while (true)
        {
            Dictionary<Coordinates, List<Coordinates>> new_path_to_point = new Dictionary<Coordinates, List<Coordinates>>();
            foreach(var item in path_to_point)
            {
                if (checked_points.Contains(item.Key))
                {
                    continue;
                }
                checked_points.Add(item.Key);

                List<Coordinates> potentional_points = new List<Coordinates>();
                potentional_points.Add(new Coordinates(item.Key.x+1, item.Key.y));
                potentional_points.Add(new Coordinates(item.Key.x, item.Key.y-1));
                potentional_points.Add(new Coordinates(item.Key.x-1, item.Key.y));
                potentional_points.Add(new Coordinates(item.Key.x, item.Key.y+1));

                foreach (var potentional_point in potentional_points)
                {
                    //Проверка, что мы не вышли за пределы зоны
                    if (!nodes_of_grapgh.Contains(potentional_point))
                    {
                        continue;
                    }
                    // Проверка, есть ли здание на пути
                    if (disabled_edges.ContainsKey(item.Key))
                    {
                        if (disabled_edges[item.Key].Contains(potentional_point))
                        {
                            continue;
                        }
                    }
                    

                    List<Coordinates> new_path = new List<Coordinates>(item.Value);
                    new_path.Add(potentional_point);
                    if (!new_path_to_point.ContainsKey(potentional_point))
                    {
                        new_path_to_point.Add(potentional_point, new_path);
                    }
                   
                }
            }
            foreach (var item in new_path_to_point)
            {
                if (!path_to_point.ContainsKey(item.Key))
                {
                    path_to_point.Add(item.Key, item.Value);
                }
               
            }


            if (new_path_to_point.Count == 0)
            {
                break;
            }
        }
    }

    private void fillDistanationPoints (Vector2Int buildingSize, Coordinates base_cell_coord)
    {
        distanation_points.Add(new Coordinates(base_cell_coord.x + (1 - Math.Sign(buildingSize.x)) / 2, base_cell_coord.y + (1 - Math.Sign(buildingSize.y)) / 2));
    }

    private void tubeBuilding()
    {
        HashSet<Coordinates> tube_box_coords = new HashSet<Coordinates>();
        foreach(var point in distanation_points)
        {
            var path = path_to_point[point];

            for (int i = 1; i < path.Count; i++)
            {
                var a = path[i - 1];
                var b = path[i];

                tube_box_coords.Add(a);
                tube_box_coords.Add(b);

                var g_o = Instantiate(tube_prefab, new Vector3((a.x + b.x) * step50 / 2.0f - step50 / 2, 0, (a.y + b.y) * step50 / 2.0f - step50 / 2), Quaternion.identity);
                g_o.transform.Rotate(new Vector3(-90, Math.Abs(a.y - b.y) * 90.0f, 0));
            }
        }
        foreach (var tube_box_coord in tube_box_coords)
        {
            var g_o = Instantiate(tube_box_prefab, new Vector3(tube_box_coord.x * step50 - step50 / 2, 0, tube_box_coord.y * step50 - step50 / 2), Quaternion.identity);
            g_o.transform.Rotate(new Vector3(-90, 0, 0));
        }
    }

}
