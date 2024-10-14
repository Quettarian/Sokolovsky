namespace Sokolovsky;

public class AccountInfo {
    /// <summary>Конструктор</summary>
    /// <param name="agreementNumber">Номер соглашения</param>
    /// <param name="accountNumber">Номер счета</param>
    /// <param name="amount">Остаток</param>
    /// <param name="currency">Код вылюты</param>
    public AccountInfo(string agreementNumber, string accountNumber, double amount, string currency) {
        AgreementNumber = Checkout(agreementNumber);
        AccountNumber = Checkout(accountNumber);
        Amount = Checkout(amount);
        Currency =  Checkout(currency);
    }

    public string AgreementNumber { get; }
    public string AccountNumber { get; }
    public double Amount { get; }
    public string Currency { get; }

    /// <summary>Проверка валидности входных параметров</summary>
    /// <param name="param">Значение входного параметра</param>
    /// <typeparam name="T">Тип входного параметра </typeparam>
    /// <exception cref="Exception">Ошибка в случае отрицательного результата проверки</exception>
    private T Checkout<T>(T param) {
        switch (param) {
            case string s when string.IsNullOrEmpty(s):
                throw new Exception($"Параметр {s} должен содержать не пустое значение!");
            case double d and < 0:
                throw new Exception($"Параметр {d} не может быть отрицательным!");
        }

        return param;
    }
}