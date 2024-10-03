namespace WebAPI.Infrastructure.Ui;

public static class UiExtensions
{
    public static string Replace(this string value, string pattern, object? item)
    {
        if (item != null)
        {
            return value.Replace(pattern, item.ToString());
        }
        else
        {
            return value.Replace(pattern, string.Empty);
        }
    }

    public static string Replace(this string value, IDictionary<string, object?> items)
    {
        foreach (var item in items)
        {
            if (item.Value != null)
            {
                value = value.Replace(item.Key, item.Value.ToString());
            }
            else
            {
                value = value.Replace(item.Key, string.Empty);
            }
        }

        return value;
    }

    public static void AddHXTriggerAfterSwap(this IHeaderDictionary dictionary, string value)
    {
        dictionary.Append("HX-Trigger-After-Swap", value);
    }

    public static void TriggerOpenModal(this IHeaderDictionary dictionary)
    {
        dictionary.Append("HX-Trigger-After-Swap", @"{""openModalEvent"":""true""}");
    }

    public static void TriggerShowSuccessMessage(this IHeaderDictionary dictionary, string message)
    {
        dictionary.Append("HX-Trigger-After-Swap", @$"{{""successMessageEvent"":""{message}""}}");
    }

    public static void TriggerShowRegisterSuccessMessage(this IHeaderDictionary dictionary, string entity, object id)
    {
        dictionary.TriggerShowSuccessMessage($"The {entity.ToLower()} {id} was registered successfully");
    }

    public static void TriggerShowEditSuccessMessage(this IHeaderDictionary dictionary, string entity, object id)
    {
        dictionary.TriggerShowSuccessMessage($"The {entity.ToLower()} {id} was edited successfully");
    }

    public static void TriggerShowSuccessMessage(this IHeaderDictionary dictionary, string entity, string action, object id)
    {
        dictionary.TriggerShowSuccessMessage($"The {entity.ToLower()} {id} was {action} successfully");
    }

    public static void TriggerShowSuccessMessageAndCloseModal(this IHeaderDictionary dictionary, string entity, string action, object id)
    {
        dictionary.TriggerShowSuccessMessageAndCloseModal($"The {entity.ToLower()} {id} was {action} successfully");
    }

    public static void TriggerShowSuccessMessageAndCloseModal(this IHeaderDictionary dictionary, string message)
    {
        dictionary.Append("HX-Trigger-After-Swap", @$"{{""successMessageEvent"":""{message}"", ""closeModalEvent"":""true""}}");
    }

    public static void TriggerShowRegisterSuccessMessageAndCloseModal(this IHeaderDictionary dictionary, string entity, object id)
    {
        dictionary.TriggerShowSuccessMessageAndCloseModal($"The {entity.ToLower()} {id} was registered successfully");
    }

    public static void TriggerShowEditSuccessMessageAndCloseModal(this IHeaderDictionary dictionary, string entity, object id)
    {
        dictionary.TriggerShowSuccessMessageAndCloseModal($"The {entity.ToLower()} {id} was edited successfully");
    }

    public static string AddListParameters(this string endpoint, int page = 1, int pageSize = 10, bool ascending = true)
    {
        return $"{endpoint}?page={page}&pageSize={pageSize}&ascending={ascending}";
    }
}
