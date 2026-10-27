const $ = (id) => document.getElementById(id);
const sleep = (ms) => new Promise((r) => setTimeout(r, ms));
let token = localStorage.getItem("nover_token");
let modus = "schnell";
let loginModus = "anmelden";
let beschaeftigt = false;

// Zentrale Funktion fuer alle API-Aufrufe
async function api(pfad, daten, ohneToken = false) {
  const kopf = { "Content-Type": "application/json" };
  if (token && !ohneToken) kopf.Authorization = "Bearer " + token;
  const r = await fetch(pfad, {
    method: daten === undefined ? "GET" : "POST",
    headers: kopf,
    body: daten === undefined ? undefined : JSON.stringify(daten),
  });
  const json = await r.json().catch(() => ({}));
  return { ok: r.ok, json };
}

// ---------- Anmeldung ----------
function setLoginModus(m) {
  loginModus = m;
  $("tab-anmelden").classList.toggle("aktiv", m === "anmelden");
  $("tab-registrieren").classList.toggle("aktiv", m === "registrieren");
  $("absenden").textContent = m === "anmelden" ? "Anmelden" : "Konto erstellen";
  $("login-meldung").textContent = "";
}

async function absenden() {
  const name = $("name").value.trim();
  const passwort = $("passwort").value;
  if (loginModus === "registrieren") {
    const r = await api("/api/register", { name, passwort }, true);
    if (!r.ok) return ($("login-meldung").textContent = r.json.fehler);
  }
  const r = await api("/api/login", { name, passwort }, true);
  if (!r.ok) return ($("login-meldung").textContent = r.json.fehler);
  token = r.json.token;
  if ($("merken").checked) localStorage.setItem("nover_token", token);
  await starteApp();
}

async function starteApp() {
  const r = await api("/api/me");
  if (!r.ok) {
    token = null;
    localStorage.removeItem("nover_token");
    return;
  }
  $("login").hidden = true;
  $("app").hidden = false;
  $("profil").textContent = `${r.json.name} · ${r.json.anzeige}`;
  zeigeTokens(r.json.tokens);
  if (!$("verlauf").children.length) {
    addMsg("nover", `Hey ${r.json.name}! Frag mich alles. Tipp /hilfe für Befehle. 😄`);
    if (r.json.bonus) addMsg("nover", "🎁 Dein Tagesbonus von 250 Tokens ist da!");
  }
  await ladeSeitenleiste();
}

async function abmelden() {
  if (token) await api("/api/logout", {});
  token = null;
  localStorage.removeItem("nover_token");
  $("verlauf").innerHTML = "";
  $("app").hidden = true;
  $("login").hidden = false;
}

// ---------- Anzeige ----------
function zeigeTokens(n) {
  $("tokens").textContent = `🪙 ${n} Tokens`;
}

function addMsg(rolle, text) {
  const box = document.createElement("div");
  box.className = "msg " + rolle;
  const kopf = document.createElement("div");
  kopf.className = "kopf";
  kopf.textContent = rolle === "du" ? "Du" : "Nover";
  const inhalt = document.createElement("div");
  inhalt.className = "text";
  inhalt.textContent = text;
  box.append(kopf, inhalt);
  $("verlauf").appendChild(box);
  $("verlauf").scrollTop = $("verlauf").scrollHeight;
  return box;
}

// Schreibt die Antwort Stueck fuer Stueck
async function tippe(el, text) {
  el.textContent = "";
  for (let i = 0; i < text.length; i += 3) {
    el.textContent += text.slice(i, i + 3);
    $("verlauf").scrollTop = $("verlauf").scrollHeight;
    await sleep(14);
  }
}

// Wartepunkte: . .. ... im Wechsel
function starteWarten(el) {
  let n = 0;
  el.textContent = ".";
  return setInterval(() => {
    n = (n % 3) + 1;
    el.textContent = "Nover denkt nach " + ".".repeat(n);
  }, 350);
}

function bewertungZeigen(box, index) {
  const zeile = document.createElement("div");
  zeile.className = "bewertung";

  const gut = document.createElement("button");
  gut.className = "icon";
  gut.textContent = "👍";
  gut.onclick = async () => {
    await api("/api/rating", { index, wert: 1 });
    zeile.textContent = "Danke für dein Feedback 🙌";
  };

  const schlecht = document.createElement("button");
  schlecht.className = "icon";
  schlecht.textContent = "👎";
  schlecht.onclick = async () => {
    await api("/api/rating", { index, wert: -1 });
    zeile.innerHTML = "";
    const frage = document.createElement("div");
    frage.className = "rueckfrage";
    frage.textContent = "Was war falsch, und wie soll ich es besser machen?";
    const feld = document.createElement("textarea");
    feld.rows = 2;
    const senden = document.createElement("button");
    senden.className = "chip aktiv";
    senden.textContent = "Senden";
    senden.onclick = async () => {
      const text = feld.value.trim();
      if (!text) return;
      const r = await api("/api/feedback", { index, text });
      zeile.textContent = r.ok ? r.json.nachricht : r.json.fehler;
    };
    zeile.append(frage, feld, senden);
  };

  zeile.append(gut, schlecht);
  box.appendChild(zeile);
}

// ---------- Senden ----------
async function senden() {
  if (beschaeftigt) return;
  const eingabe = $("frage");
  const frage = eingabe.value.trim();
  if (!frage) return;
  eingabe.value = "";
  addMsg("du", frage);

  if (frage.startsWith("/")) {
    const cmd = frage.toLowerCase();
    if (cmd === "/abmelden") return abmelden();
    const r = await api("/api/command", { text: frage });
    if (cmd === "/neu") $("verlauf").innerHTML = "";
    addMsg("nover", r.ok ? r.json.antwort : r.json.fehler);
    await ladeSeitenleiste();
    return;
  }

  beschaeftigt = true;
  $("senden").disabled = true;
  const box = addMsg("nover", "");
  const text = box.querySelector(".text");
  const timer = starteWarten(text);

  const r = await api("/api/chat", { frage, schwer: modus === "komplex" });
  clearInterval(timer);
  beschaeftigt = false;
  $("senden").disabled = false;

  if (!r.ok) {
    text.textContent = r.json.fehler;
    if (r.json.verbleibend !== undefined) zeigeTokens(r.json.verbleibend);
    return;
  }
  zeigeTokens(r.json.verbleibend);
  await tippe(text, r.json.antwort);
  bewertungZeigen(box, r.json.index);
  await ladeSeitenleiste();
}

function setModus(m) {
  modus = m;
  $("m-schnell").classList.toggle("aktiv", m === "schnell");
  $("m-komplex").classList.toggle("aktiv", m === "komplex");
}

async function neuesGespraech() {
  await api("/api/new", {});
  $("verlauf").innerHTML = "";
  addMsg("nover", "Neues Gespräch gestartet. Was brauchst du? 🎮");
  zeigeAnsicht("chat");
}

// ---------- Seitenleiste und Dashboard ----------
async function ladeSeitenleiste() {
  const r = await api("/api/stats");
  if (!r.ok) return;
  zeigeTokens(r.json.tokens);
  const liste = $("letzte");
  liste.innerHTML = "";
  if (!r.json.letzte.length) liste.textContent = "Noch keine Fragen.";
  r.json.letzte.forEach((f) => {
    const d = document.createElement("div");
    d.className = "letzte-item";
    d.textContent = f.length > 28 ? f.slice(0, 27) + "…" : f;
    liste.appendChild(d);
  });
}

function zeigeAnsicht(name) {
  const chat = name === "chat";
  $("ansicht-chat").hidden = !chat;
  $("ansicht-dash").hidden = chat;
  $("titel").textContent = chat ? "Chat" : "Dashboard";
  $("nav-chat").classList.toggle("aktiv", chat);
  $("nav-dash").classList.toggle("aktiv", !chat);
  if (!chat) ladeDashboard();
}

async function ladeDashboard() {
  const s = await api("/api/stats");
  if (!s.ok) return;
  const d = s.json;
  $("k-tokens").textContent = d.tokens;
  $("k-gespraeche").textContent = d.gespraeche;
  $("k-zufrieden").textContent = d.zufrieden === null ? "–" : d.zufrieden + "%";
  $("k-schwer").textContent = d.schwer + "%";

  const balken = $("balken");
  balken.innerHTML = "";
  const max = Math.max(1, ...d.tage.map((t) => t.anzahl));
  d.tage.forEach((t) => {
    const spalte = document.createElement("div");
    spalte.className = "spalte";
    const stab = document.createElement("div");
    stab.className = "stab";
    stab.style.height = (t.anzahl / max) * 100 + "%";
    stab.title = t.anzahl + " Gespräche";
    const label = document.createElement("div");
    label.className = "tag";
    label.textContent = t.tag;
    spalte.append(stab, label);
    balken.appendChild(spalte);
  });

  const liste = $("liste");
  liste.innerHTML = "";
  d.letzte.forEach((f) => {
    const li = document.createElement("li");
    li.textContent = f;
    liste.appendChild(li);
  });
}

// ---------- Ereignisse ----------
$("tab-anmelden").onclick = () => setLoginModus("anmelden");
$("tab-registrieren").onclick = () => setLoginModus("registrieren");
$("absenden").onclick = absenden;
$("passwort").addEventListener("keydown", (e) => { if (e.key === "Enter") absenden(); });
$("senden").onclick = senden;
$("frage").addEventListener("keydown", (e) => { if (e.key === "Enter") senden(); });
$("m-schnell").onclick = () => setModus("schnell");
$("m-komplex").onclick = () => setModus("komplex");
$("neu").onclick = neuesGespraech;
$("nav-chat").onclick = () => zeigeAnsicht("chat");
$("nav-dash").onclick = () => zeigeAnsicht("dash");
$("abmelden").onclick = abmelden;

// Token nach Google/GitHub-Anmeldung aus der Adresse uebernehmen
const parameter = new URLSearchParams(location.search);
if (parameter.get("token")) {
  token = parameter.get("token");
  localStorage.setItem("nover_token", token);
  history.replaceState(null, "", "/");
}

setLoginModus("anmelden");
if (token) starteApp();
