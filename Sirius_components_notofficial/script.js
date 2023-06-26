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
    aggiornaTabella();
    mostraOrarioPreciso();
    Calcolini()
    CreaPersone()
    inserisciTab()
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
    for (i = 0; i < persone.length; i++) {
        coso = []
        for (j = 0; j < personeUniche.length; j++) {
            if (persone[i] == personeUniche[j]) {
                pezzo = []
                coso.push(new Pezzi(data[i].idReadable, data[i].fields.priority, data[i].fields.dueDate, data[i].fields.startDate, data[i].fields.workEffort, data[i].fields.state))
                pezzo.push(persone[i].nome, persone[i].team, coso)
                listaTab1.push(pezzo)
            }
        }
    }
    /*for (i = 0; i < personeUniche.length; i++) {
        for (j = 0; j < persone.length; j++) {
            if (persone[j] == personeUniche[i]) {
                coso = []
                pezzo = []
                coso.push(new Pezzi(data[j].idReadable, data[j].fields.priority, data[j].fields.dueDate, data[j].fields.startDate, data[j].fields.workEffort, data[j].fields.state))
                pezzo.push(persone[j].nome, persone[j].team, coso)
                listaTab1.push(pezzo)
            }
        }
    }*/
    console.log(listaTab1)
}

/*==================Crea Tabella========================= */
function inserisciTab(){
    /*'<div><div class="data"><div class="name"></div><div class="information"></div></div></div>'*/
    tab = document.getElementById("sinistro").innerHTML = ""
    let string = ""
    for(i = 0; i<listaTab1.length; i++){
        string += '<div><div class="data"><div class="name">'+ listaTab1[i][0] + '</div><div class="information">' + listaTab1[i][1] +'</div></div></div>'
    }
    document.getElementById("sinistro").innerHTML = string
}

function inseriscidiv() {
    for(i = 0; i<persone.length * ; i++){
        string += '<div></div>';
    }
}


