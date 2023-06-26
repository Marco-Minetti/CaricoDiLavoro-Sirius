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
class PersonaTab {
    constructor(nome, team) {
        this.nome = nome
        this.team = team
    }
    aggiungi(item){
        this.coso.push(item)
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
    aggiornaTabella();
    mostraOrarioPreciso();
    Calcolini()
    CreaPersone()
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
    // ho paura di doverlo fare globale
    coso = []
    for (i = 0; i < personeUniche.length; i++) {
        for (j = 0; j < persone.length; j++) {
            if (persone[j] == personeUniche[i]) {
                
                coso.push(new Pezzi(data[j].idReadable, data[j].fields.priority, data[j].fields.dueDate, data[j].fields.startDate, data[j].fields.workEffort, data[j].fields.state))
                listaTab1.push(persone[j].nome, persone[j].team, coso)
            }
        }
    }
}



