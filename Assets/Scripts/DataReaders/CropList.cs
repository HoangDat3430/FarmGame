using System.Collections.Generic;

public class CropList : CSVReader
{
    private Dictionary<int, RowData> cropDic = new Dictionary<int, RowData>();

    public class RowData
    {
        public int CropID;
        public int Stages;
        public string RuleTile;
        public int GrowthTime;
        public int HarvestNum;
        public int StageAfterHarvest;
        public int ProductPerHarvest;
        public int DieTime;
        public int ProductID;
    }
    public RowData[] Table = null;
    public override void InitRowData(int line, string[] data)
    {
        Table[line] = new RowData();
        Table[line].CropID = int.Parse(data[0]);
        Table[line].Stages = int.Parse(data[1]);
        Table[line].RuleTile = data[2];
        Table[line].GrowthTime = int.Parse(data[3]);
        Table[line].HarvestNum = int.Parse(data[4]);
        Table[line].StageAfterHarvest = int.Parse(data[5]);
        Table[line].ProductPerHarvest = int.Parse(data[6]);
        Table[line].DieTime = int.Parse(data[7]);
        Table[line].ProductID = int.Parse(data[8]);
        cropDic[Table[line].CropID] = Table[line];
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
    public RowData GetByCropId(int cropId)
    {
        return cropDic[cropId];
    }
}
