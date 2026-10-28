namespace colibri_api.Common.Persistence;

public interface IHasRowVersion
{
    byte[] RowVersion { get; set; }
}
