public class CropList : CSVReader
{
    public class RowData
    {
        public int cropID;
        public int stages;
        public string ruleTile;
        public int growthTime;
        public int harvestNum;
        public int stageAfterHarvest;
        public int productPerHarvest;
        public int dieTime;
    }
    public RowData[] Table = null;
    public override void InitRowData(int line, string[] data)
    {
        Table[line] = new RowData();
        Table[line].cropID = int.Parse(data[0]);
        Table[line].stages = int.Parse(data[1]);
        Table[line].ruleTile = data[2];
        Table[line].growthTime = int.Parse(data[3]);
        Table[line].harvestNum = int.Parse(data[4]);
        Table[line].stageAfterHarvest = int.Parse(data[5]);
        Table[line].productPerHarvest = int.Parse(data[6]);
        Table[line].dieTime = int.Parse(data[7]);
    }
    public override void OnClear(int lineCount)
    {
        if (lineCount > 0)
        {
            Table = new RowData[lineCount];
        }
        else
        {
            Table = null;
        }
    }
}
