using Microsoft.EntityFrameworkCore;
using NexaLearn.DTOs.Datatable;
using System.Linq.Dynamic.Core;

namespace NexaLearn.Datatable
{
    public class DataTableService
    {
        /// <summary>
        /// Servicio para procesar peticiones DataTables con paginación, ordenamiento y filtro global.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<DataTableResponse<T>> GetPagedDataAsync<T>(
            IQueryable<T> query,
            DataTableRequest request)
        {
            int totalRecords = await query.CountAsync();
            var entityType = typeof(T);
            var columns = request.Columns ?? new List<DataTableColumn>();

            // --------------------------------------------------------------------
            // FILTRO GLOBAL (solo columnas string)
            // --------------------------------------------------------------------
            var searchableStringColumns = columns
                .Where(c => c.Searchable)
                .Where(c =>
                {
                    if (c.Data == null)
                        return false;

                    var prop = entityType.GetProperty(c.Data);
                    return prop != null && prop.PropertyType == typeof(string);
                })
                .Select(c => c.Data)
                .ToList();

            if (!string.IsNullOrWhiteSpace(request.Search?.Value) && searchableStringColumns.Any())
            {
                var searchValue = request.Search.Value;

                // Genera:  Nombre.Contains(@0) OR Descripcion.Contains(@0)
                var filterExpression = string.Join(" OR ",
                    searchableStringColumns.Select(col => $"{col}.Contains(@0)")
                );

                query = query.Where(filterExpression, searchValue);
            }

            // --------------------------------------------------------------------
            // ORDENAMIENTO
            // --------------------------------------------------------------------
            if (request.Order?.Any() == true)
            {
                var order = request.Order.First();
                var colName = order.Column >= 0 && order.Column < columns.Count
                    ? columns[order.Column].Data
                    : null;

                if (!string.IsNullOrEmpty(colName))
                {
                    var direction = order.Dir?.ToLower() == "desc" ? "desc" : "asc";
                    query = query.OrderBy($"{colName} {direction}");
                }
            }

            int filteredRecords = await query.CountAsync();

            // --------------------------------------------------------------------
            // PAGINACIÓN
            // --------------------------------------------------------------------
            var length = request.Length < 0 ? 100 : Math.Min(request.Length, 1000);
            var dataList = await query
                .Skip(Math.Max(0, request.Start))
                .Take(length)
                .ToListAsync();

            // --------------------------------------------------------------------
            // RESPUESTA COMPATIBLE CON DATATABLES (minúsculas)
            // --------------------------------------------------------------------
            return new DataTableResponse<T>
            {
                draw = request.Draw,
                recordsTotal = totalRecords,
                recordsFiltered = filteredRecords,
                data = dataList
            };
        }

        /// <summary>
        /// Procesa una colección temporal con el mismo contrato de DataTables.
        /// Se utiliza para vistas previas que todavía no deben persistirse.
        /// </summary>
        public Task<DataTableResponse<T>> GetPagedDataAsync<T>(
            IEnumerable<T> source,
            DataTableRequest request)
        {
            var query = source.AsQueryable();
            var totalRecords = query.Count();
            var entityType = typeof(T);
            var columns = request.Columns ?? new List<DataTableColumn>();

            var searchableStringColumns = columns
                .Where(c => c.Searchable)
                .Where(c => !string.IsNullOrWhiteSpace(c.Data))
                .Where(c => entityType.GetProperty(c.Data!)?.PropertyType == typeof(string))
                .Select(c => c.Data!)
                .ToList();

            if (!string.IsNullOrWhiteSpace(request.Search?.Value) && searchableStringColumns.Any())
            {
                var filterExpression = string.Join(" OR ",
                    searchableStringColumns.Select(col => $"{col}.Contains(@0)"));
                query = query.Where(filterExpression, request.Search.Value);
            }

            var order = request.Order?.FirstOrDefault();
            if (order != null && order.Column >= 0 && order.Column < columns.Count)
            {
                var columnName = columns[order.Column].Data;
                if (!string.IsNullOrWhiteSpace(columnName) && entityType.GetProperty(columnName) != null)
                {
                    var direction = string.Equals(order.Dir, "desc", StringComparison.OrdinalIgnoreCase)
                        ? "desc"
                        : "asc";
                    query = query.OrderBy($"{columnName} {direction}");
                }
            }

            var filteredRecords = query.Count();
            var length = request.Length < 0 ? 100 : Math.Min(request.Length, 1000);
            var data = length == 0
                ? []
                : query.Skip(Math.Max(0, request.Start)).Take(length).ToList();

            return Task.FromResult(new DataTableResponse<T>
            {
                draw = request.Draw,
                recordsTotal = totalRecords,
                recordsFiltered = filteredRecords,
                data = data
            });
        }
    }
}
