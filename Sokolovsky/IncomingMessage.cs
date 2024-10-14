﻿using System.Text.Json.Serialization;

namespace Sokolovsky;

public class IncomingMessage {
    public IncomingMessage(Request request, AccountInfo debitPart, AccountInfo creditPart, string details, string bankingDate, Attributes attributes) {
        Request = request;
        DebitPart = debitPart;
        CreditPart = creditPart;
        Details = details;
        BankingDate = bankingDate;
        Attributes = attributes;
    }

    public Request Request { get; }
    public AccountInfo DebitPart { get; }
    public AccountInfo CreditPart { get; }
    public string Details { get; }
    public string BankingDate { get; }
    public Attributes Attributes { get; }
}

public struct Request(long id, Document document) {
    public long Id { get; } = id;
    public Document Document { get; } = document;
}

public struct Document(long id, string type) {
    public long Id { get; } = id;
    public string Type { get; } = type;
}

public struct Attribute(string code, string attribute) {
    public string Code { get; } = code;
    [JsonPropertyName("attribute")]
    public string Name { get; } = attribute;
}

public struct Attributes(List<Attribute> attribute) {
    public List<Attribute> Attribute { get; } = attribute;
}