using System.ComponentModel;

namespace Matrix.Domain.Enums;

public enum DiseaseEnum
{
    // Leves;
    [Description("Resfriado comum")]
    Cold,

    [Description("Gripe")]
    Flu,

    [Description("Intoxicação alimentar")]
    FoodPoisoning,

    [Description("Enxaqueca")]
    Migraine,

    [Description("Alergia")]
    Allergy,

    // Moderadas;
    [Description("Pneumonia")]
    Pneumonia,

    [Description("Bronquite")]
    Bronchitis,

    [Description("Asma")]
    Asthma,

    [Description("Diabetes")]
    Diabetes,

    [Description("Hipertensão arterial")]
    Hypertension,

    // Graves;
    [Description("Doença cardíaca")]
    HeartDisease,

    [Description("Acidente vascular cerebral")]
    Stroke,

    [Description("Insuficiência renal")]
    KidneyFailure,

    [Description("Doença hepática")]
    LiverDisease,

    [Description("Tuberculose")]
    Tuberculosis,

    // Cânceres;
    [Description("Câncer de pulmão")]
    LungCancer,

    [Description("Câncer cerebral")]
    BrainCancer,

    [Description("Câncer de pele")]
    SkinCancer,

    [Description("Câncer de mama")]
    BreastCancer,

    [Description("Câncer de próstata")]
    ProstateCancer,

    // Mentais;
    [Description("Transtorno de ansiedade")]
    AnxietyDisorder,

    [Description("Depressão")]
    Depression,

    [Description("Síndrome de burnout")]
    Burnout,

    [Description("Transtorno do pânico")]
    PanicDisorder,

    [Description("Transtorno bipolar")]
    BipolarDisorder,

    // Extremamente graves;
    [Description("Doença de Alzheimer")]
    Alzheimer,

    [Description("Doença de Parkinson")]
    Parkinsons,

    // Terminais;
    [Description("Câncer terminal")]
    TerminalCancer
}