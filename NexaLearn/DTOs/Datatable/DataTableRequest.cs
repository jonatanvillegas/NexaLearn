namespace NexaLearn.DTOs.Datatable
{
    public class DataTableRequest
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; } = 6;
        public DataTableSearch Search { get; set; } = new();
        public List<DataTableOrder>? Order { get; set; }
        public List<DataTableColumn> Columns { get; set; } = [];
    }


    public class DataTableSearch
    {
        public string? Value { get; set; }
        public bool Regex { get; set; }
    }


    public class DataTableOrder
    {
        public int Column { get; set; }
        public string Dir { get; set; } = "asc";
    }


    public class DataTableColumn
    {
        public string Data { get; set; } = string.Empty;
        public string? Name { get; set; }
        public bool Searchable { get; set; }
        public bool Orderable { get; set; }
        public DataTableSearch Search { get; set; } = new();
    }
}
