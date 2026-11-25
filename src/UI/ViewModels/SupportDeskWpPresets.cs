using TextProcessor.Logic.AbstractSyntaxTree;

namespace UI.ViewModels;

internal static class SupportDeskWpPresets
{
    internal static IReadOnlyList<WpPreset> All { get; } =
    [
        new(
            Title: "Какая очередь длиннее",
            Code: """
                if (vpnQueue >= mailQueue) {
                    longestQueue := vpnQueue;
                } else {
                    longestQueue := mailQueue;
                }
                """,
            PostCondition: "longestQueue > 100",
            Description: "Сравниваем очереди заявок по VPN и почте. Цель: в большей из них больше 100 заявок."),
        new(
            Title: "Сколько заявок обработаем за смену",
            Code: """
                if (discriminant >= 0) {
                    batchSize := (-minutesPerTicket + rootDiscriminant) / (2 * growth);
                    hasEstimate := 1;
                } else {
                    batchSize := -1;
                    hasEstimate := 0;
                }
                """,
            PostCondition: "(hasEstimate == 1 && batchSize >= 0 && growth * batchSize * batchSize + minutesPerTicket * batchSize + timeGap == 0) || (hasEstimate == 0 && batchSize == -1)",
            Description: "Оцениваем размер пакета заявок по учебной модели времени. timeGap — минуты подготовки минус длительность смены. На вход подаём rootDiscriminant — корень из дискриминанта или 0, если он отрицательный."),
        new(
            Title: "Два снимка очереди VPN",
            Code: """
                firstSnapshot := vpnQueue + 1;
                vpnQueue := vpnQueue + 10;
                """,
            PostCondition: "firstSnapshot == vpnQueue - 9 && vpnQueue > 15",
            Description: "В очередь поступает пакет из десяти заявок. Сохраняем число заявок после первой и после всего пакета. Цель: разница между снимками равна девяти, а итог больше 15.")
    ];
}
