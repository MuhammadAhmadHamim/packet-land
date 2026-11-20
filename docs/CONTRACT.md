# PacketLand Contract v1

Frozen on Day 1. After that: additive changes only (add fields, never rename or remove),
through a pull request the other member approves.

## 1. Transport
- TCP, default host 127.0.0.1, default port 5000.
- Frame = 4-byte big-endian length + UTF-8 JSON. Length 1..65536. Anything else is a violation and the connection is closed.
- One writer per socket at a time.
- Heartbeat: client sends PING every 5 s, server replies PONG. 15 s of silence = disconnect.

## 2. Envelope
{ "v":1, "type":"INTENT", "seq":12, "action":"ADD_RULE", "payload":{...} }
- v: protocol version, must be 1.
- type: one of the message types below.
- seq: client counter, echoed as refSeq in ERROR.
- action: only on INTENT.
- payload: JSON object. Absent for PING, PONG, RETRY and FLUSH_CACHE.

## 3. Message types
| Type | Direction | Payload |
|---|---|---|
| JOIN | client to server | JoinMessage |
| WELCOME | server to client | WelcomeMessage |
| INTENT | client to server | per action, see section 5 |
| SNAPSHOT | server to client | SnapshotMessage, every 100 ms |
| EVENT | server to client | EventMessage |
| ALERT | server to client | AlertMessage |
| ERROR | server to client | ErrorMessage |
| ROUND_END | server to client | RoundEndMessage |
| PING / PONG | both | none |

## 4. Session flow
1. Connect and send JOIN with a name and a role.
2. Server replies WELCOME, or ERROR ROLE_TAKEN.
3. SNAPSHOTs start immediately (phase Waiting).
4. When all 5 roles have joined: Countdown (5 s), then Running (120 s).
5. ROUND_END is sent, and the phase becomes Ended. One round per server run in v1.

## 5. Roles and intents
| Role | Action | Payload | Rules |
|---|---|---|---|
| Client | START_REQUEST | domain | domain must be example.com |
| Client | RETRY | none | only after a failed or timed-out attempt |
| Router | SET_LINK_COST | linkId, cost | cost 1 to 20 |
| Router | SET_LINK_STATE | linkId, up | links L1 to L8 |
| Firewall | ADD_RULE | rule | id ignored, server assigns it; new rule goes to the top; limitPerSecond 1 to 1000; max 20 rules |
| Firewall | REMOVE_RULE | ruleId | must exist |
| Firewall | SET_DEFAULT | action | Allow or Deny only |
| DNS | FLUSH_CACHE | none | |
| DNS | SET_VALIDATION | enabled | |
| Attacker | SYN_FLOOD | rate, spoof | rate 1 to 500 packets per second |
| Attacker | POISON_DNS | domain, fakeIp | domain must be example.com |
| Attacker | DEGRADE_LINK | linkId | links L1 to L8 |

An action outside the sender's role gets ERROR NOT_YOUR_ACTION. There is no success reply:
success is visible in the next SNAPSHOT. Attack cooldowns are enforced by the server.

## 6. Errors
NOT_JOINED, ROLE_TAKEN, NOT_YOUR_ACTION, UNKNOWN_ACTION, BAD_PAYLOAD, OUT_OF_RANGE,
ON_COOLDOWN, ROUND_NOT_RUNNING. Each carries refSeq when tied to an intent.

## 7. Snapshots
- Exactly one panel is non-null: the receiver's own role.
- nodes and links: the same for every role.
- packets are filtered per role:
  - Client: its own flow.
  - Router: packets on links L1, L2, L3, L7, L8.
  - Firewall: packets on links L4, L5, L6.
  - DNS: packets on link L7.
  - Attacker: packets it sent.
- suspicious comes from the server's detection heuristics, never from ground truth.
- progress is 0 to 1 along the link at that tick. Clients extrapolate using latencyMs and tickMs.
- A dropped packet is shown fading for one snapshot, then removed.
- The hidden truth label never leaves the server.

## 8. Topology
| Node | Address | Played by |
|---|---|---|
| CLIENT | 10.0.0.2 | Client |
| R1 | gateway | Router |
| R2, R3 | transit | automatic |
| FW | in front of WEB | Firewall |
| WEB | 203.0.113.10 | simulation |
| DNS | 198.51.100.53 | DNS |
| ATK | 192.0.2.66 | Attacker (terminal) |

| Link | Between | Base cost |
|---|---|---|
| L1 | CLIENT, R1 | 1 |
| L2 | R1, R2 | 1 |
| L3 | R1, R3 | 2 |
| L4 | R2, FW | 1 |
| L5 | R3, FW | 2 |
| L6 | FW, WEB | 1 |
| L7 | R1, DNS | 1 |
| L8 | R1, ATK | 1 |

## 9. Command-line arguments
| Program | Arguments |
|---|---|
| Server | --port 5000 --autostart --seed 42 --simulate file.json --dump-snapshots dir |
| Client (Avalonia) | --host 127.0.0.1 --port 5000 --roles Client,Router,Firewall,Dns |
| AttackerCli | --host 127.0.0.1 --port 5000 --name attacker |

## 10. Change rules
- Core/ and this file change only through a pull request approved by the other member.
- Never rename or remove a field, only add. Bump "v" only for a breaking change both agree on.
- Every new message type needs a round-trip test in Core.Tests.