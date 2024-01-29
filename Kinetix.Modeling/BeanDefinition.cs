using System.ComponentModel.DataAnnotations;
using Kinetix.Modeling.Exceptions;

namespace Kinetix.Modeling;

/// <summary>
/// Définition d'un bean.
/// </summary>
[Serializable]
public class BeanDefinition
{
    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="beanType">Type du bean.</param>
    /// <param name="properties">Collection de propriétés.</param>
    /// <param name="contractName">Nom du contrat (table).</param>
    /// <param name="isReference"><code>True</code> si le bean correspond à une liste de référence, <code>False</code> sinon.</param>
    /// <param name="isStatic"><code>True</code> si le bean correspond à une liste de référence statique, <code>False</code> sinon.</param>
    internal BeanDefinition(Type beanType, BeanPropertyDescriptorCollection properties, string contractName, bool isReference, bool isStatic)
    {
        BeanType = beanType;
        Properties = properties;
        ContractName = contractName;
        IsReference = isReference;
        IsStatic = isStatic;
        foreach (var property in properties)
        {
            if (property.IsPrimaryKey && PrimaryKey == null)
            {
                PrimaryKey = property;
            }

            if (property.IsDefault)
            {
                DefaultProperty = property;
            }
        }

        PrimaryKey ??= properties.First();

    }

    /// <summary>
    /// Retourne le type du bean.
    /// </summary>
    public Type BeanType
    {
        get;
        private set;
    }

    /// <summary>
    /// Retourne le nom du contrat.
    /// </summary>
    public string ContractName
    {
        get;
        private set;
    }

    /// <summary>
    /// Retourne <code>True</code> si le bean est une liste de référence, <code>False</code> sinon.
    /// </summary>
    public bool IsReference
    {
        get;
        private set;
    }

    /// <summary>
    /// Retourne <code>True</code> si le Bean est une liste statique, <code>False</code> sinon.
    /// </summary>
    public bool IsStatic
    {
        get;
        private set;
    }

    /// <summary>
    /// Retourne la propriété par défaut si elle existe.
    /// </summary>
    public BeanPropertyDescriptor DefaultProperty
    {
        get;
        private set;
    }

    /// <summary>
    /// Retourne la clef primaire si elle existe.
    /// </summary>
    public BeanPropertyDescriptor PrimaryKey
    {
        get;
        private set;
    }

    /// <summary>
    /// Retourne la liste des propriétés d'un bean.
    /// </summary>
    public BeanPropertyDescriptorCollection Properties
    {
        get;
        private set;
    }

    /// <summary>
    /// Vérifie les contraintes sur un bean.
    /// </summary>
    /// <param name="bean">Bean à vérifier.</param>
    /// <param name="propertiesToCheck">Si renseigné, seules ces propriétés seront validées.</param>
    /// <returns>Les erreurs.</returns>
    internal ErrorMessageCollection GetErrors(object bean, IEnumerable<string> propertiesToCheck = null)
    {
        var errors = new ErrorMessageCollection();
        foreach (var property in Properties.Where(prop => propertiesToCheck == null || propertiesToCheck.Contains(prop.PropertyName)))
        {
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateProperty(property.GetValue(bean), new ValidationContext(bean) { MemberName = property.PropertyName }, validationResults);
            foreach (var valRes in validationResults)
            {
                errors.AddEntry(new ErrorMessage(property.PropertyName, valRes.ErrorMessage)
                {
                    ModelName = BeanType.Name
                });
            }

            if (property.DomainName == null || property.IsReadOnly)
            {
                continue;
            }

            foreach (var error in property.CheckDomain(property.GetValue(bean)))
            {
                errors.AddEntry(error);
            }
        }

        return errors;
    }

    /// <summary>
    /// Vérifie les contraintes sur un bean.
    /// </summary>
    /// <param name="bean">Bean à vérifier.</param>
    /// <param name="propertiesToCheck">Si renseigné, seules ces propriétés seront validées.</param>
    internal void Check(object bean, IEnumerable<string> propertiesToCheck = null)
    {
        var errors = GetErrors(bean, propertiesToCheck);
        if (errors.Any())
        {
            throw new BusinessException(errors);
        }
    }
}
