using System;
using System.CodeDom;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Reflection;

namespace GroupControls.Design
{
	/// <summary>
	/// Specialized serializer for a <see cref="T:System.Windows.Forms.Control"/> that ensures that <c>SuspendLayout()</c> and <c>ResumeLayout()</c> are called in the designer.
	/// </summary>
	internal class DesignerLayoutCodeDomSerializer : CodeDomSerializer
	{
		/// <summary>
		/// Serializes the specified object into a CodeDOM object.
		/// </summary>
		/// <param name="manager">The serialization manager to use during serialization.</param>
		/// <param name="value">The object to serialize.</param>
		/// <returns>
		/// A CodeDOM object representing the object that has been serialized.
		/// </returns>
		public override object Serialize(IDesignerSerializationManager manager, object value)
		{
			// Invoke the default serializer for base class
			var baseSerializer = (CodeDomSerializer)manager.GetSerializer(typeof(ControlListBase).BaseType, typeof(CodeDomSerializer));
			var statements = baseSerializer.Serialize(manager, value) as CodeStatementCollection;

			// Add layout methods
			if (statements != null)
			{
				SerializeMethodInvocation(manager, statements, value, "SuspendLayout", null, new Type[0], true);

				var parameters = new CodeExpressionCollection { new CodePrimitiveExpression(true) };
				var paramTypes = new[] { typeof(bool) };
				SerializeMethodInvocation(manager, statements, value, "ResumeLayout", parameters, paramTypes, false);
			}

			// Return the new statements
			return statements;
		}

		private void SerializeMethodInvocation(IDesignerSerializationManager manager, CodeStatementCollection statements, object control, string methodName, CodeExpressionCollection parameters, Type[] paramTypes, bool preorder)
		{
			manager.GetName(control);
			paramTypes = ToTargetTypes(control, paramTypes);
			if (TypeDescriptor.GetReflectionType(control).GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance, null, paramTypes, null) != null)
			{
				var expression2 = new CodeMethodReferenceExpression(SerializeToExpression(manager, control), methodName);
				var expression = new CodeMethodInvokeExpression { Method = expression2 };
				if (parameters != null)
					expression.Parameters.AddRange(parameters);
				var statement = new CodeExpressionStatement(expression);
				if (preorder)
					statement.UserData["statement-ordering"] = "begin";
				else
					statement.UserData["statement-ordering"] = "end";
				statements.Add(statement);
			}
		}

		private static Type ToTargetType(object context, Type runtimeType) => TypeDescriptor.GetProvider(context).GetReflectionType(runtimeType);

		private static Type[] ToTargetTypes(object context, Type[] runtimeTypes)
		{
			var typeArray = new Type[runtimeTypes.Length];
			for (var i = 0; i < runtimeTypes.Length; i++)
				typeArray[i] = ToTargetType(context, runtimeTypes[i]);
			return typeArray;
		}
	}
}
