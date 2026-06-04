using System.ComponentModel;

namespace Matrix.Domain.Enums;

public enum CauseOfDeathEnum
{
    [Description("Morte por causas naturais")]
    NaturalCauses,

    [Description("Morte decorrente da idade avançada")]
    OldAge,

    [Description("Morte causada por uma doença")]
    Disease,

    [Description("Morte causada por câncer")]
    Cancer,

    [Description("Morte causada por ataque cardíaco")]
    HeartAttack,

    [Description("Morte causada por um acidente")]
    Accident,

    [Description("Morte causada por acidente automobilístico")]
    CarAccident,

    [Description("Morte causada por homicídio")]
    Murder,

    [Description("Morte causada por fome extrema")]
    Starvation,

    [Description("Morte causada por overdose de drogas")]
    DrugOverdose,

    [Description("Morte causada por depressão")]
    Depression,

    [Description("Causa da morte desconhecida")]
    Unknown
}