Black Sniffer
A Windows network packet analyzer built with Uno Platform and SharpPcap.
 Requirements
Npcap Driver (Required)
NetworkParser requires Npcap to capture network packets. Without it the application will not be able to capture any traffic.

Download Npcap from the official website: https://npcap.com/#download
Run the installer
Restart your computer if prompted


Note: WinPcap is no longer maintained and is not supported. Use Npcap instead.

System Requirements

OS: Windows 10 / Windows 11 (64-bit)
Runtime: .NET 10
Privileges: Administrator rights required for packet capture


Features

Live Packet Capture — capture traffic on any network interface in promiscuous mode
Packet List — real-time table with #, Time, Source, Destination, Protocol, Length, Info columns
Protocol Color Coding — rows colored by protocol (TCP, UDP, ARP, ICMP, TLS)
Packet Details — expandable protocol tree (Ethernet → IP → TCP/UDP/ICMP/ARP/TLS)
Hex Viewer — raw bytes of selected packet
Display Filter — filter packets by protocol, IP address, port
Search — search across all visible packets by any field
Follow TCP Stream — reconstruct and display TCP session payload
Statistics — top protocols, top source IPs, top conversations, capture summary
Save / Open — save captures to .pcap and open existing .pcap / .pcapng files
DNS Resolution — resolves IP addresses to hostnames in packet details
TLS Detection — detects TLS handshake and extracts SNI (Server Name Indication)
Packet Crafter — send custom packets:

ARP Spoofing
TCP RST injection
Raw hex packet editor




Installation

Install Npcap
Download the latest release from Releases
Extract and run NetworkParser.exe as Administrator


Building from Source
bash# Clone the repository
git clone https://github.com/yourname/NetworkParser.git
cd NetworkParser

# Restore dependencies
dotnet restore

# Build
dotnet build -f net10.0-desktop

# Run
dotnet run --project NetworkParser.UI -f net10.0-desktop
Dependencies

Uno Platform — cross-platform UI framework
SharpPcap — packet capture library
PacketDotNet — packet parsing library
CommunityToolkit DataGrid — data grid control


Usage
Capturing Traffic

Launch the application as Administrator
Click Interfaces to select a network interface
Click Start to begin capture
Click Stop to stop

Filtering
Type in the filter box to filter displayed packets:

tcp — show only TCP packets
udp — show only UDP packets
192.168 — show packets containing this IP
443 — show packets on port 443

Follow TCP Stream
Double-click any TCP row to open the TCP stream viewer for that session.
Packet Crafter
Click Crafter to open the packet injection tool.

⚠️ Use only on networks you own or have explicit permission to test.


⚠️ Legal Notice
NetworkParser is intended for educational purposes and authorized network diagnostics only.
Using packet injection features (ARP spoofing, TCP RST) on networks without explicit permission is illegal in most jurisdictions. The authors are not responsible for any misuse.

Architecture
NetworkParser/
├── NetworkParser.Core              # Packet capture, parsing, filtering
│   ├── Connection/                 # NetworkParserController (SharpPcap)
│   └── Helpers/                    # PacketFilterBuilder, TlsParser
├── NetworkParser.Core.Abstractions # Interfaces
├── NetworkParser.Domain            # Models (PacketModel, ProtocolModel)
└── NetworkParser.UI                # Uno Platform UI
    ├── ViewModels/                 # MVVM ViewModels
    └── Views/                     # XAML Pages, Controls, Dialogs

License
MIT License — see LICENSE for details.
