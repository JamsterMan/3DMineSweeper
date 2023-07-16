using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game3D : MonoBehaviour
{
    private Tile[,,] grid;
    private GameObject[,,] objectGrid;
    public int width = 10;
    public int height = 10;
    public int depth = 3;
    private int currentDepth = 0;
    public int numMines = 12;
    public int numFlags = 0;
    public int numCoveredTiles = 0;
    private int xOld, yOld = -1;
    private bool gameOver = false;
    private bool middleFunc = false;
    private bool firstReveal = true;
    public Text mineCounter;
    public ChangeZoom zoomCam;


    void Start()
    {
        //PlaceMines(1, 1);

        grid = new Tile[width, height, depth];
        objectGrid = new GameObject[width, height, depth];
        SetStartValues();

        for (int i = 0; i < width; i++){
            for (int j = 0; j < height; j++){
                for (int k = 0; k < depth; k++) {
                    PlaceTiles(i, j, k);
                }
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        BoardActions();   
        if (!gameOver) {
            TileActions();
        }
        
    }

    //takes mouse input to decide what to do the the tile at the current mouse position
    void TileActions()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        int x = Mathf.RoundToInt(mousePosition.x);
        int y = Mathf.RoundToInt(mousePosition.y);
        if (x >= 0 && x < width && y >= 0 && y < height) {//check if mouse was in the game field
            if (EventSystem.current.IsPointerOverGameObject())//stop clicks through UI
                return;
            Tile tile = grid[x, y, currentDepth];
            if (Input.GetButtonDown("Fire1")) {//reveal a tile on left mouse click
                if (firstReveal) {
                    PlaceMines(x, y);
                    firstReveal = false;
                }
                if (tile.isCovered && !tile.isFlaged) {//cant reveal a flaged tile
                    tile.RevealTile();
                    numCoveredTiles--;
                    if (tile.type == Tile.TileType.Blank) {
                        RevealNeighbors(x, y);
                    }
                    else if (tile.type == Tile.TileType.Mine) {//game over
                        tile.HitMine();
                        GameLost();
                    }
                    CheckWin();
                }
            }
            else if (Input.GetButtonDown("Fire2")) {//flag a tile on right mouse click
                if (tile.isCovered) {//cant flag revealed tiles
                    if (tile.isFlaged) {//if tile is already flaged
                        numFlags--;
                        numCoveredTiles++;//flags are not counted as coveredTiles to make win check simpler
                        tile.UnflagTile();
                    }
                    else {
                        numFlags++;
                        numCoveredTiles--;//flags are not counted as coveredTiles to make win check simpler
                        tile.FlagTile();
                    }
                    SetMineCounterText(numMines - numFlags);
                    CheckWin();
                }
            }
            else if (Input.GetButtonDown("Fire3")) {//turn on highlight on middle mouse button pressed down
                middleFunc = true;
                HighlightNeighbors(x, y);
                xOld = x;
                yOld = y;
            }
            else if (Input.GetButtonUp("Fire3")) {//turn off highlight on middle mouse button release
                middleFunc = false;
                UnHighlightNeighbors(x, y);
            }

        }
        if (middleFunc) {//highlight surronding uncovered tiles, and reveal tile if correct number of flags present
            if (xOld != x || yOld != y) {//check if mouse has moved from last position
                if (xOld >= 0 && yOld >= 0)
                    UnHighlightNeighbors(xOld, yOld);
                if (x >= 0 && x < width && y >= 0 && y < height) { //check if mouse was in the game field
                    HighlightNeighbors(x, y);
                    xOld = x;
                    yOld = y;
                }
                else {
                    if (Input.GetButtonUp("Fire3")) {//turn off highlight
                        middleFunc = false;
                        xOld = -1;
                        yOld = -1;
                    }
                }
            }
        }
    }

    //change witch layer of the game board the player is looking at
    void BoardActions()
    {
        if (Input.GetButtonDown("Up")) {//press 'q' to go up one layer up
            if (currentDepth < depth -1) {
                ChangeDepth(currentDepth + 1);
            }
        }
        else if (Input.GetButtonDown("Down")) {//press 'e' to go down one layer up
            if (currentDepth > 0) {
                ChangeDepth(currentDepth - 1);
            }
        }
    }

    //changes the layer visable to the player, takes in the new layer to show
    void ChangeDepth(int newDepth)
    {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                objectGrid[x, y, currentDepth].GetComponent<Renderer>().enabled = false;
                objectGrid[x, y, newDepth].GetComponent<Renderer>().enabled = true;
                //objectGrid[x, y, currentDepth].SetActive(false);
                //objectGrid[x, y, newDepth].SetActive(true);
            }
        }
        currentDepth = newDepth;
        Debug.Log(currentDepth);
    }

    //reveals tiles around x, y
    void RevealNeighbors(int x, int y)
    {
        for (int xOff = -1; xOff <= 1; xOff++) {
            for (int yOff = -1; yOff <= 1; yOff++) {
                if (x + xOff > -1 && x + xOff < width && y + yOff > -1 && y + yOff < height) {//for coner tiles to not compare with tiles that dont exist
                    if (!grid[x + xOff, y + yOff, 0].isFlaged) {//dont reveal if the player miss flagged something
                        if (grid[x + xOff, y + yOff, currentDepth].isCovered) {//check if tile is covered
                            numCoveredTiles--;
                            grid[x + xOff, y + yOff, currentDepth].RevealTile();
                            if (grid[x + xOff, y + yOff, currentDepth].type == Tile.TileType.Blank) {
                                RevealNeighbors(x + xOff, y + yOff);
                            }else if (grid[x + xOff, y + yOff, currentDepth].type == Tile.TileType.Mine) {
                                grid[x + xOff, y + yOff, currentDepth].HitMine();
                                GameLost();
                            }
                        }
                    }
                }
            }
        }
        CheckWin();
    }

    //highlights tiles around x, y
    void HighlightNeighbors(int x, int y)
    {
        if (!grid[x, y, 0].isFlaged) {//dont highlight a flag tiles neighbors
            int totalFlags = 0;
            for (int xOff = -1; xOff <= 1; xOff++) {
                for (int yOff = -1; yOff <= 1; yOff++) {
                    if (x + xOff > -1 && x + xOff < width && y + yOff > -1 && y + yOff < height) {//for coner tiles
                        if (!grid[x + xOff, y + yOff, 0].isFlaged) {//dont highlight if flagged
                            if (grid[x + xOff, y + yOff, 0].isCovered) {
                                grid[x + xOff, y + yOff, 0].HighlightTile();
                            }
                        } else {
                            totalFlags++;
                        }
                    }
                }
            }
            if (grid[x, y, 0].type == Tile.TileType.Num && grid[x, y, 0].mineNeighbors == totalFlags) {//reveals neighbor tiles if the correct number of flags present
                RevealNeighbors(x, y);
                middleFunc = false;
            }
        }
    }
    
    //unhighlighs tiles around x, y
    void UnHighlightNeighbors(int x, int y)
    {
        for (int xOff = -1; xOff <= 1; xOff++) {
            for (int yOff = -1; yOff <= 1; yOff++) {
                if (x + xOff > -1 && x + xOff < width && y + yOff > -1 && y + yOff < height) {//for coner tiles
                    if (!grid[x + xOff, y + yOff, 0].isFlaged) {//dont highlight if flagged
                        if (grid[x + xOff, y + yOff, 0].isCovered) {
                            grid[x + xOff, y + yOff, 0].UnHighlightTile();
                        }
                    }
                }
            }
        }
    }

    //reveals mines and mistake after losing
    void GameLost()
    {
        //game over
        Debug.Log("Defeat");
        gameOver = true;//stops revealing more tiles after gameover
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                for (int z = 0; z < depth; z++) {
                    if (grid[i, j, z].type == Tile.TileType.Mine && grid[i, j, 0].isCovered && !grid[i, j, 0].isFlaged) {//check if all flags are right
                        grid[i, j, z].RevealTile();
                    }
                    if (grid[i, j, z].type != Tile.TileType.Mine && grid[i, j, 0].isFlaged) {//check if all flags are right
                        grid[i, j, z].MissFlag();
                    }
                }
            }
        }
    }

    //checks if the player has flagged all the mines or revealed all non mines
    void CheckWin()
    {
        /*if (numFlags + numCoveredTiles == numMines || numFlags == numMines) {//checks if win condition met
            Debug.Log("Checking win");
            int correctFlags = 0;
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    if (grid[i, j, 0].type == Tile.TileType.Mine && grid[i, j, 0].isFlaged) {//check if all flags are right
                        correctFlags++;
                    }
                }
            }
            if (correctFlags == numFlags) {
                if (correctFlags == numMines || correctFlags + numCoveredTiles == numMines) {
                    Debug.Log("Victory");

                    FindObjectOfType<AudioManager>().Play("Victory");//plays a victory sound

                    gameOver = true;//stop unflaging and revealing after a win
                    for (int i = 0; i < width; i++) {//flag tile that are not flagged yet
                        for (int j = 0; j < height; j++) {
                            if (grid[i, j, 0].isCovered && !grid[i, j, 0].isFlaged) {
                                if (grid[i, j, 0].type == Tile.TileType.Mine) {//counts remaining covered flags
                                    grid[i, j, 0].FlagTile();
                                } else {
                                    grid[i, j, 0].RevealTile();
                                }
                            }
                        }
                    }
                }
            }
        }*/
    }

    //Resets the gameboard
    public void ResetGame()
    {
        ClearTiles();
        SetStartValues();
        Debug.Log("Game Board Reset");

        FindObjectOfType<AudioManager>().Play("NewBoard");
    }

    //sets all tiles back to empty tiles
    void ClearTiles()
    {
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                for (int z = 0; z < depth; z++) {
                    grid[i, j, z].SetTile("Empty", Tile.TileType.Blank);
                    grid[i, j, z].CoverTile();
                }
            }
        }
    }

    //Creates a new game board
    public void NewBoard(int newWidth, int newHeight, int newMines)
    {
        ClearBoard();
        width = newWidth;
        height = newHeight;
        numMines = newMines;
        grid = new Tile[width, height, 0];
        objectGrid = new GameObject[width, height, depth];
        zoomCam.ZoomCameraOnBoard();
        MakeNewBoard();
        SetStartValues();//sets values used to tell win/loss to starting vals
        Debug.Log("New Game Board Made");

        FindObjectOfType<AudioManager>().Play("NewBoard");
    }
    
    //clears the board to change board size
    void ClearBoard()
    {
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                for (int k = 0; k < depth; k++) {
                    Destroy(objectGrid[i, j, k]);
                    Destroy(grid[i, j, k]);
                }
            }
        }
    }

    //remakes board of width and height
    void MakeNewBoard()
    {
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                for (int k = 0; k < depth; k++) {
                    PlaceTiles(i, j, k);
                }
            }
        }
    }



    /* Sets tiles to mines
     * mouseX and mouseY are the tile clicked that cant be a mine
     */
    void PlaceMines(int mouseX, int mouseY)
    {
        string[,,] charGrid = new string[width,height, depth];
        int x, y , z;
        for (int m = 0; m < numMines; m++) { // place mines in the field
            x = Random.Range(0, width);
            y = Random.Range(0, height);
            z = Random.Range(0, depth);

            while (charGrid[x, y, z] == "M" || (x == mouseX && y == mouseY)){//grid[x, y].type == Tile.TileType.Mine || (x == mouseX && y == mouseY)) {//makes sure no mine placements overlap
                x = Random.Range(0, width);
                y = Random.Range(0, height);
                z = Random.Range(0, depth);
            }
            grid[x, y, z].SetTile("Mine", Tile.TileType.Mine);
            charGrid[x, y, z] = "M";
        }

        //Set the count for number of adjactent mines
        for (z = 0; z < depth; z++) {
            for (x = 0; x < width; x++) {
                for (y = 0; y < height; y++) {

                    if (charGrid[x, y, z] != "M") {//grid[x, y].type != Tile.TileType.Mine){
                        int total = 0;

                        for (int xOff = -1; xOff <= 1; xOff++) {
                            for (int yOff = -1; yOff <= 1; yOff++) {
                                for (int zOff = -1; zOff <= 1; zOff++) {
                                    if (x + xOff > -1 && x + xOff < width && y + yOff > -1 && y + yOff < height && z + zOff > -1 && z + zOff < depth) {//for corner tiles
                                        if (charGrid[x + xOff, y + yOff, z + zOff] == "M") {//grid[x + xOff, y + yOff].type == Tile.TileType.Mine) {
                                            total++;
                                        }
                                    }
                                }
                            }
                        }
                        if (total == 0) {
                            grid[x, y, z].SetTile("Empty", Tile.TileType.Blank);
                            charGrid[x, y, z] = "0";
                        }
                        else {
                            grid[x, y, z].SetTile("" + total, Tile.TileType.Num);
                            grid[x, y, z].SetMineNeighbors(total);
                            charGrid[x, y, z] = "" + total;
                        }
                    }
                }
            }
        }
        string board = "";
        for (z = 0; z < depth; z++){
            for (x = 0; x < width; x++){
                for (y = 0; y < height; y++){
                    board += charGrid[x, y, z] + " ";
                }
                board += "\n";
            }
            board += "\n\n";
        }
        Debug.Log(board);
    }

    void PlaceTiles(int x, int y, int z)
    {
        //hide game objects that should not be seen (all but current depth)
        GameObject gameObject = Instantiate(Resources.Load("Prefabs/Empty", typeof(GameObject)), new Vector3(x, y, 0), Quaternion.identity) as GameObject;
        Tile tile = gameObject.GetComponent<Tile>();

        objectGrid[x, y, z] = gameObject;
        grid[x, y, z] = tile;

        if(z != currentDepth) {//hide tiles of different layers
            objectGrid[x, y, z].GetComponent<Renderer>().enabled = false;
            //objectGrid[x, y, z].SetActive(false);
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }

    private void SetStartValues()
    {
        firstReveal = true;//makes it so mines are place after the next click
        gameOver = false;//allows player to click again
        numCoveredTiles = width * height;//used for win calculation
        numFlags = 0;//used for win calculation
        xOld = -1;
        yOld = -1;
        SetMineCounterText(numMines);
    }

    private void SetMineCounterText(int mineCount)
    {
        mineCounter.text = "" + mineCount;
    }
    
}
