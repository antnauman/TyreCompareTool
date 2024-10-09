namespace TyreCompare.Models;

public class ModelPage<Model>
{
    public List<Model> ModelList { get; set; }

    public PaginationMeta PageMeta { get; set; }

    public ModelPage(List<Model> modelList, PaginationMeta paginationInfo)
    {
        ModelList = modelList;
        PageMeta = paginationInfo;
    }
}