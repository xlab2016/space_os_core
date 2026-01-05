using Data.Repository;
using SpaceCore.Data.SpaceCoreDb.Entities;

namespace SpaceCore.Models.Queries.ProjectedPoints
{
    public partial class ProjectedPointQuery : QueryBase<Data.SpaceCoreDb.Entities.ProjectedPoint, ProjectedPointFilter, ProjectedPointSort>
    {
    }
}
