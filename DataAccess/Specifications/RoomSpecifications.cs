using System;
using System.Linq.Expressions;
using BusinessObject.Entities;
using BusinessObject.DTOs.RoomDTOs;

namespace DataAccess.Specifications
{
    public static class RoomSpecifications
    {
        public static Expression<Func<Room, bool>> ByFilter(RoomFilterDto? filter)
        {
            // Start with true
            Expression<Func<Room, bool>> spec = r => true;

            if (filter == null) return spec;

            if (!string.IsNullOrWhiteSpace(filter.BuildingId))
            {
                var buildingId = filter.BuildingId;
                spec = spec.AndAlso(r => r.BuildingID == buildingId);
            }

            if (!string.IsNullOrWhiteSpace(filter.RoomTypeId))
            {
                var roomTypeId = filter.RoomTypeId;
                spec = spec.AndAlso(r => r.RoomTypeID == roomTypeId);
            }

            if (filter.Price.HasValue)
            {
                var price = filter.Price.Value;
                spec = spec.AndAlso(r => r.RoomType.Price >= price);
            }

            return spec;
        }

        // helper to combine expressions
        private static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            var param = Expression.Parameter(typeof(T));

            var leftVisitor = new ReplaceParameterVisitor(first.Parameters[0], param);
            var left = leftVisitor.Visit(first.Body);

            var rightVisitor = new ReplaceParameterVisitor(second.Parameters[0], param);
            var right = rightVisitor.Visit(second.Body);

            var body = Expression.AndAlso(left!, right!);
            return Expression.Lambda<Func<T, bool>>(body, param);
        }

        private class ReplaceParameterVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _oldParam;
            private readonly ParameterExpression _newParam;

            public ReplaceParameterVisitor(ParameterExpression oldParam, ParameterExpression newParam)
            {
                _oldParam = oldParam;
                _newParam = newParam;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (node == _oldParam) return _newParam;
                return base.VisitParameter(node);
            }
        }
    }
}
