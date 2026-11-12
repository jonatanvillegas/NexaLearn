namespace NexaLearn.DTOs.Datatable
{
    public class DataTableResponse<T>
    {
        /// <summary>
        /// Copia exacta del parámetro 'draw' recibido, requerido por DataTables.
        /// </summary>
        public int draw { get; set; }

        /// <summary>
        /// Total de registros sin filtros.
        /// </summary>
        public int recordsTotal { get; set; }

        /// <summary>
        /// Total de registros después de aplicar filtros.
        /// </summary>
        public int recordsFiltered { get; set; }

        /// <summary>
        /// Lista de datos.
        /// </summary>
        public List<T> data { get; set; } = [];
    }
}
