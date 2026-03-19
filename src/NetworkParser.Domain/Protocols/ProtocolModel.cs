using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace NetworkParser.Domain.Protocols;

public class ProtocolModel {
    public string DisplayText { get; set; }

    public ObservableCollection<ProtocolModel> Children { get; }
        = new ObservableCollection<ProtocolModel>();

    public ProtocolModel (string text) {
        DisplayText = text;
    }
}
