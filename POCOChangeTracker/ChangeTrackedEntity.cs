using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace POCOChangeTracker
{
	public class ChangeTrackedPOCO<TEntity>
	{
		private ChangeTrackedPOCO<TEntity> oldValue { get; set; }

		public void StartChangeTracking()
		{
			oldValue = (ChangeTrackedPOCO<TEntity>)Activator.CreateInstance(GetType());
			CopyProperties(this, oldValue);
		}

		public void RejectChanges()
		{
			CopyProperties(oldValue, this);
		}

		public void AcceptChanges()
		{
			StartChangeTracking();
		}

		public object GetOldValue<TProperty>(Expression<Func<TEntity, TProperty>> propertyLambda)
		{
			Type type = typeof(TEntity);

			MemberExpression member = propertyLambda.Body as MemberExpression;
			if (member == null)
				throw new ArgumentException($"Expression '{propertyLambda.ToString()}' should refer to a property!");

			PropertyInfo propInfo = member.Member as PropertyInfo;
			if (propInfo == null)
				throw new ArgumentException($"Expression '{propertyLambda.ToString()}' should refer to a property!");

			if (type != propInfo.ReflectedType &&
				!type.IsSubclassOf(propInfo.ReflectedType))
				throw new ArgumentException($"Expression '{propertyLambda.ToString()}' should refer to a property of type {type}.");

			return propInfo.GetValue(oldValue);
		}

		public List<string> GetChangedProperties()
		{
			var type = GetType();

			var allProps = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly)
				.Where(pi => pi.PropertyType.IsSimpleType());

			var diffProperties =
				from pi in allProps
				let thisPropValue = type.GetProperty(pi.Name).GetValue(this, null)
				let oldOPropValue = type.GetProperty(pi.Name).GetValue(oldValue, null)
				where thisPropValue != oldOPropValue && !Equals(thisPropValue, oldOPropValue)
				select pi.Name;

			return diffProperties.ToList();
		}

		private static ChangeTrackedPOCO<TEntity> CopyProperties(ChangeTrackedPOCO<TEntity> source, ChangeTrackedPOCO<TEntity> target)
		{
			foreach (var sProp in source.GetType().GetProperties())
			{
				bool isMatched = target.GetType().GetProperties().Any(tProp => tProp.Name == sProp.Name && tProp.GetType() == sProp.GetType() && tProp.CanWrite);
				if (isMatched)
				{
					var value = sProp.GetValue(source);
					PropertyInfo propertyInfo = target.GetType().GetProperty(sProp.Name);
					propertyInfo.SetValue(target, value);
				}
			}
			return target;
		}
	}

	public static class Ext
	{
		public static bool IsSimpleType(this Type type)
		{
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				return type.GetGenericArguments()[0].IsSimpleType();
			}
			return type.IsPrimitive
			  || type.IsEnum
			  || type.Equals(typeof(string))
			  || type.Equals(typeof(decimal));
		}
	}
}
