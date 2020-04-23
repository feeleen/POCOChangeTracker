using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace POCOChangeTracker
{
	public class ChangeTrackedPOCO<TEntity> : IChangeTracking
	{
		private int GenerationsCounter { get; set; } = 0;
		private ChangeTrackedPOCO<TEntity> oldValue { get; set; }

		public bool IsChanged => GetChangedProperties().Count > 0;

		public int GetGenerationsCount()
		{
			return GenerationsCounter;
		}

		public void RejectChanges()
		{
			CopyProperties(oldValue, this);
		}

		public void AcceptChanges()
		{
			if (oldValue == null)
			{
				oldValue = (ChangeTrackedPOCO<TEntity>)Activator.CreateInstance(GetType());
			}

			CopyProperties(this, oldValue);
			GenerationsCounter++;
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

			if (oldValue == null)
				return allProps.Select(x => x.Name).ToList();

			var diffProperties =
				from pi in allProps
				let thisPropValue = type.GetProperty(pi.Name).GetValue(this, null)
				let oldOPropValue = type.GetProperty(pi.Name).GetValue(oldValue, null)
				where thisPropValue != oldOPropValue && !Equals(thisPropValue, oldOPropValue)
				select pi.Name;

			return diffProperties == null? allProps.Select(x => x.Name).ToList() : diffProperties.ToList();
		}

		private static ChangeTrackedPOCO<TEntity> CopyProperties(ChangeTrackedPOCO<TEntity> source, ChangeTrackedPOCO<TEntity> target)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			if (target == null)
				throw new ArgumentNullException(nameof(target));

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
