namespace API.Dto;

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int Size, int Total, int TotalPages);
