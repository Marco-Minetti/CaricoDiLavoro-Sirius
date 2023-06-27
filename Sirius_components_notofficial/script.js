//Variabili globali
let dati;
persone = []
personeUniche = []
listaTab1 = []
class Person {
    constructor(name, group) {
        this.nome = name
        this.team = group
    }
}
class Pezzi {
    constructor(id, prio, dueDate, endDate, workEffort, state) {
        this.id = id
        this.prio = prio
        this.dueDate = dueDate
        this.endDate = endDate
        this.workEffort = workEffort
        this.state = state
    }
}





$(document).ready(async function () {
    dati = await leggiticket();
    mostraOrarioPreciso();
    Calcolini();
    CreaPersone();
    inserisciTab();
    aggiornaTabella();
});

function mostraOrarioPreciso() {
    var data = new Date();

    var giorniSettimana = ["Domenica", "Lunedì", "Martedì", "Mercoledì", "Giovedì", "Venerdì", "Sabato"];
    var giorno = giorniSettimana[data.getDay()];

    var numeroGiorno = data.getDate();

    var mesi = ["Gennaio", "Febbraio", "Marzo", "Aprile", "Maggio", "Giugno", "Luglio", "Agosto", "Settembre", "Ottobre", "Novembre", "Dicembre"];
    var mese = mesi[data.getMonth()];

    var anno = data.getFullYear();

    var ore = data.getHours();
    var minuti = data.getMinutes();
    var secondi = data.getSeconds();

    var orario = giorno + " " + numeroGiorno + " " + mese + " " + anno + " - Ora " + ore + ":" + minuti + ":" + secondi;

    document.getElementById("orarioPreciso").innerHTML = orario;
}

setInterval(mostraOrarioPreciso, 1000); // Aggiorna l'orario ogni secondo




function aggiornaTabella() {

    var table = new Tabulator("#projectTable", {
        data: dati,
        height: "311px",
        columns: [
            { title: "ID", field: "idReadable" },
            { title: "Responsabile", field: "fields.assignee" },
            { title: "Titolo", field: "summary" },
            { title: "StartDate", field: "fields.startDate" },
            { title: "EndDate", field: "fields.dueDate" },
            { title: "WorkEffort", field: "fields.workEffort" },
        ],
    });

}

async function leggiticket() {
    let file = 'https://localhost:7075/Dati';
    const response = await fetch(file);
    data = await response.json();



    return data;
}


/*==============Calcolini===============*/
function Calcolini() {
    for (i = 0; i < data.length; i++) {
        if (data[i].fields.assignee != null && data[i].fields.team) {
            persone.push(new Person(data[i].fields.assignee, data[i].fields.team))
        }
    }
    personeUniche = persone.filter(
        (person, index, self) =>
            index === self.findIndex((p) => p.nome === person.nome)
    )
}
function CreaPersone() {
    for (i = 0; i < personeUniche.length; i++) {
        pezzo = []
        coso = []
        let nome
        let team
        for (j = 0; j < persone.length; j++) {
            if (persone[j].nome == personeUniche[i].nome && persone[j].team == personeUniche[i].team) {
                coso.push(new Pezzi(data[j].idReadable, data[j].fields.priority, data[j].fields.dueDate, data[j].fields.startDate, data[j].fields.workEffort, data[j].fields.state))
                nome = persone[j].nome
                team = persone[j].team
            }
        }
        pezzo.push(nome, team, coso)
        listaTab1.push(pezzo)
    }
}

/*==================Crea Tabella========================= */
function inserisciTab() {
    /*'<div><div class="data"><div class="name"></div><div class="information"></div></div></div>'*/
    tab = document.getElementsByClassName("third")[0].innerHTML = ""
    let string = ""
    for (i = 0; i < listaTab1.length; i++) {
        string += '<div><div class="data"><div class="name">' + listaTab1[i][0] + '</div><div class="information">' + listaTab1[i][1] + '</div></div></div>'
    }
    document.getElementsByClassName("third")[0].innerHTML = string
}

function SetDay(giorni) {
    const temp = document.querySelector('.grid');
    temp.style.gridTemplateColumns = "repeat(" + giorni + ", 1fr);"
    inseriscidiv(giorni);
    inseriscigiorni(giorni);
}

function inseriscidiv(giorni) {
    document.getElementsByClassName("third_")[0].innerHTML = ""
    let string = "";
    for (i = 0; i < listaTab1.length * giorni; i++) {
        string += '<div></div>'
    }
    document.getElementsByClassName("third_")[0].innerHTML = string
}

function inseriscigiorni(giorni) {
    document.getElementsByClassName("project")[0].innerHTML = "";
    let string = "";
    for (i = 1; i <= giorni; i++) {
        string += '<div><div>' + i + '</div></div>'
    }
    document.getElementsByClassName("project")[0].innerHTML = string
}
function handleSelection(){
    var giorni = document.getElementById("days").value;
    const temp = document.querySelector('.grid');
    temp.style.gridTemplateColumns = "repeat(" + giorni + ", 1fr);"
    inseriscidiv(giorni);
    inseriscigiorni(giorni);
}

