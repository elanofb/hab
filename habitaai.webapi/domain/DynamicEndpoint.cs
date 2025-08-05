using System;

namespace habitaai.webapi.domain
{
    public class DynamicEndpoint
    {
        public int Id { get; set; }
        public string Route { get; set; } = default!;
        public string? RouteLast { get; set; } = default!;
        public string? SourceCodeFull { get; set; } = default!;
        public string SourceCode { get; set; } = default!;
        public string MethodName { get; set; } = "Handle";
        public bool Enabled { get; set; } = true;
        public string? RouteUniqueId { get; set; } = default!;
        public string? RequestType { get; set; } = default!;
        public string? InputText { get; set; } = default!;
        public string? ParamsInfo { get; set; } = default!;
        public string? PayloadJson { get; set; } = default!;
        public string? FlowJson { get; set; } = default!;
        public string? Description { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}