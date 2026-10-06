namespace LastEpoch_Hud.Scripts.Core.CustomItems;

/// <summary>Mjolner description text from its settings, per language.</summary>
public static class MjolnerDescription
{
    public static string LightningProc(
        string language,
        int strRequirement,
        int intRequirement,
        float minTriggerChance,
        float maxTriggerChance
    )
    {
        if (string.IsNullOrEmpty(language))
        {
            return "";
        }

        int minChance = (int)((minTriggerChance / 255f) * 100f);
        int maxChance = (int)((maxTriggerChance / 255f) * 100f);
        return language switch
        {
            "fr" => FrenchProc(strRequirement, intRequirement, minChance, maxChance),
            "de" => GermanProc(strRequirement, intRequirement, minChance, maxChance),
            "pt" => PortugueseProc(strRequirement, intRequirement, minChance, maxChance),
            _ => EnglishProc(strRequirement, intRequirement, minChance, maxChance),
        };
    }

    public static string SocketedSkills(
        string language,
        int strRequirement,
        int intRequirement,
        double socketedCooldownMs,
        string skill0,
        string skill1,
        string skill2
    )
    {
        if (string.IsNullOrEmpty(language))
        {
            return "";
        }

        double cooldownSeconds = socketedCooldownMs / 1000;
        if (language == "fr")
        {
            return FrenchSocketed(
                strRequirement,
                intRequirement,
                cooldownSeconds,
                skill0,
                skill1,
                skill2
            );
        }

        return EnglishSocketed(
            strRequirement,
            intRequirement,
            cooldownSeconds,
            skill0,
            skill1,
            skill2
        );
    }

    private static string EnglishProc(int str, int intel, int minChance, int maxChance)
    {
        return "If you have at least "
            + str
            + " Strength and "
            + intel
            + " Intelligence, "
            + minChance
            + " to "
            + maxChance
            + "% chance to Trigger a Lightning Spell on Hit with an Attack";
    }

    private static string FrenchProc(int str, int intel, int minChance, int maxChance)
    {
        return "Si vous avez au moins "
            + str
            + " de Force et "
            + intel
            + " d'Intelligence, "
            + minChance
            + " à "
            + maxChance
            + "% de chance de déclencher un sort de foudre lors d'une attaque réussie";
    }

    private static string GermanProc(int str, int intel, int minChance, int maxChance)
    {
        return "Wenn Sie mindestens "
            + str
            + " Stärke und "
            + intel
            + " Intelligenz haben, "
            + minChance
            + " bis "
            + maxChance
            + "% Chance, bei Treffer mit einem Angriff einen Blitzzauber auszulösen";
    }

    private static string PortugueseProc(int str, int intel, int minChance, int maxChance)
    {
        return "Se você tiver pelo menos "
            + str
            + " de Força e "
            + intel
            + " de Inteligência, ganhe "
            + minChance
            + " a "
            + maxChance
            + "% de chance para Ativar uma Magia de Raio ao Acertar um Ataque";
    }

    private static string EnglishSocketed(
        int str,
        int intel,
        double cooldown,
        string skill0,
        string skill1,
        string skill2
    )
    {
        return "If you have at least "
            + str
            + " Strength and "
            + intel
            + " Intelligence, Trigger "
            + skill0
            + ", "
            + skill1
            + " and "
            + skill2
            + " on Hit, with a "
            + cooldown
            + " second Cooldown";
    }

    private static string FrenchSocketed(
        int str,
        int intel,
        double cooldown,
        string skill0,
        string skill1,
        string skill2
    )
    {
        return "Si vous avez au moins "
            + str
            + " de Force et "
            + intel
            + " d'Intelligence, déclenche "
            + skill0
            + ", "
            + skill1
            + " et "
            + skill2
            + " à l'impact, avec un temps de recharge de "
            + cooldown
            + " seconde";
    }
}
