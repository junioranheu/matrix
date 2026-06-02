using System.ComponentModel;

namespace Matrix.Domain.Enums;

public enum MoodEnum
{
    // Extremamente positivos;
    [Description("Sentindo uma felicidade extrema")]
    Ecstatic,

    [Description("Tomado por uma sensação intensa de euforia")]
    Euphoric,

    [Description("Em completo estado de felicidade e bem-estar")]
    Blissful,

    [Description("Cheio de inspiração e criatividade")]
    Inspired,

    [Description("Sentindo-se vitorioso após uma grande conquista")]
    Triumphant,

    // Positivos;
    [Description("Animado e cheio de energia")]
    Excited,

    [Description("Motivado para alcançar objetivos")]
    Motivated,

    [Description("Feliz")]
    Happy,

    [Description("Confiante de que tudo dará certo")]
    Optimistic,

    [Description("Esperançoso em relação ao futuro")]
    Hopeful,

    [Description("Orgulhoso de si mesmo ou de uma conquista")]
    Proud,

    [Description("Confiante em suas capacidades")]
    Confident,

    [Description("Satisfeito com a situação atual")]
    Satisfied,

    [Description("Relaxado e sem preocupações")]
    Relaxed,

    [Description("Calmo e emocionalmente estável")]
    Calm,

    [Description("Sentindo-se amado e valorizado")]
    Loved,

    [Description("Movido por forte paixão ou entusiasmo")]
    Passionate,

    // Neutros;
    [Description("Sem emoções predominantes")]
    Neutral,

    [Description("Refletindo sobre pensamentos ou situações")]
    Thoughtful,

    [Description("Interessado em descobrir ou aprender algo")]
    Curious,

    [Description("Concentrado em uma tarefa ou objetivo")]
    Focused,

    [Description("Sem interesse ou envolvimento emocional")]
    Indifferent,

    // Levemente negativos;
    [Description("Entediado pela falta de estímulos")]
    Bored,

    [Description("Fisicamente ou mentalmente cansado")]
    Tired,

    [Description("Com sono")]
    Sleepy,

    [Description("Com dificuldade de manter a atenção")]
    Distracted,

    [Description("Inquieto ou incapaz de relaxar")]
    Restless,

    [Description("Impaciente com pessoas ou situações")]
    Impatient,

    [Description("Com ciúmes de alguém")]
    Jealous,

    [Description("Invejando algo que outra pessoa possui")]
    Envious,

    [Description("Constrangido por uma situação")]
    Embarrassed,

    [Description("Sentindo culpa por algo que fez ou deixou de fazer")]
    Guilty,

    // Negativos;
    [Description("Sentindo-se sozinho")]
    Lonely,

    [Description("Triste")]
    Sad,

    [Description("Decepcionado com uma situação ou resultado")]
    Disappointed,

    [Description("Frustrado por não alcançar um objetivo")]
    Frustrated,

    [Description("Levemente irritado")]
    Irritated,

    [Description("Bravo ou irritado")]
    Angry,

    [Description("Extremamente furioso")]
    Furious,

    [Description("Sentindo vergonha de si mesmo")]
    Ashamed,

    [Description("Com baixa autoconfiança")]
    Insecure,

    [Description("Esperando resultados negativos")]
    Pessimistic,

    // Relacionamentos;
    [Description("Sofrendo por um coração partido")]
    Heartbroken,

    [Description("Sentindo-se rejeitado")]
    Rejected,

    [Description("Sentindo-se traído por alguém")]
    Betrayed,

    [Description("Em processo de luto")]
    Grieving,

    [Description("Sentindo saudade do passado")]
    Nostalgic,

    // Trabalho e vida adulta;
    [Description("Mentalmente esgotado pelo excesso de responsabilidades")]
    BurnedOut,

    [Description("Sobrecarregado de trabalho")]
    Overworked,

    [Description("Incapaz de lidar com todas as demandas atuais")]
    Overwhelmed,

    [Description("Sob elevado nível de estresse")]
    Stressed,

    [Description("Sentindo forte pressão externa")]
    Pressured,

    [Description("Sem vontade de agir ou produzir")]
    Unmotivated,

    // Saúde mental;
    [Description("Ansioso")]
    Anxious,

    [Description("Preocupado com algo")]
    Worried,

    [Description("Nervoso diante de uma situação")]
    Nervous,

    [Description("Em estado de pânico")]
    Panicked,

    [Description("Deprimido")]
    Depressed,

    [Description("Sem esperança em relação ao futuro")]
    Hopeless,

    // Medo;
    [Description("Assustado")]
    Scared,

    [Description("Aterrorizado")]
    Terrified,

    [Description("Excessivamente desconfiado ou paranoico")]
    Paranoid,

    // Estados raros;
    [Description("Confuso e sem compreender totalmente a situação")]
    Confused,

    [Description("Chocado por um acontecimento inesperado")]
    Shocked,

    [Description("Maravilhado ou impressionado")]
    Amazed,

    [Description("Determinado a alcançar um objetivo")]
    Determined,

    [Description("Obcecado por uma pessoa, ideia ou objetivo")]
    Obsessed
}