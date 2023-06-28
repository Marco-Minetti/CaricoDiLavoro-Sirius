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
    constructor(id, prio, dueDate, start, workEffort, state, avg) {
        this.id = id
        this.prio = prio
        this.dueDate = dueDate
        this.startDate = start
        this.workEffort = workEffort
        this.state = state
        this.avg = avg
    }
}

$(document).ready(async function () {
    dati = await leggiticket();
    mostraOrarioPreciso();
    Calcolini();
    CreaPersone();
    inserisciTab();
    creaTabella();
    handleSelection();
    //creaGiorni(); //mettere il valore di inizio della tabella e quello di fine
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




function creaTabella() {

    var table = new Tabulator("#projectTable", {
        data: dati,
        selectable:true,
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
    //table.selectRow(1);
    //var selector_table = table.selectRow("data[0].fields.startDate > data[0].fields.dueDate");
    //selector_table.style.backgroundColor = 'red';

    //selector_table = data.getData().startDate > data.getData().dueDate;

}

async function leggiticket() {
    let file = 'https://localhost:7075/Dati';
    const response = await fetch(file);
    data = await response.json();
    for(i = 0; i < data.length; i++) {
        if ( data[i].fields.startDate > data[i].fields.dueDate ) {
            data[i].style= "true";
        }
        else {
            data[i].style= "false";
        }
        if (data[i].style = "true") {
            var r = data[i].select();
            r.style = "backgrownd-color: red";
        }
    }



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
                coso.push(new Pezzi(data[j].idReadable, data[j].fields.priority, data[j].fields.dueDate, data[j].fields.startDate, data[j].fields.workEffort, data[j].fields.state,
                    getAvg(data[j].fields.startDate, data[j].fields.dueDate, data[j].fields.workEffort)
                ))
                nome = persone[j].nome
                team = persone[j].team
            }
        }
        pezzo.push(nome, team, coso)
        listaTab1.push(pezzo)
    }
}
function getAvg(start, end, minutes) {
    data1 = new Date(start);
    data2 = new Date(end);
    let numeroGiorni = 0;
    while (data1 < data2) {
        if (checkFestivo(data1)) { numeroGiorni++; }
        data1.setDate(data1.getDate() + 1);
    }
    if (numeroGiorni == 0) { return 0 } // aggiungere messaggio di errore
    return (minutes / 60 / numeroGiorni)

}
function checkFestivo(data) {
    data = new Date(data);
    giornoSettimana = data.getDay();
    if (giornoSettimana == 0 || giornoSettimana == 6) {   //0 = domenica, 6 = sabato
        return false;
    } else { return true; }
}

function creaGiorni(inizio, fine) {
    for (i = 0; i < listaTab1.length; i++) {
        oreGiornata = dataCorretta(inizio, fine, listaTab1[i][2].startDate, listaTab1[i][2].dueDate);
        console.log(oreGiornata)
    }


}
function dataCorretta(inizio, fine, startDate, dueDate) {
    start = new Date(inizio)
    end = new Date(fine)
    startDate = new Date(startDate)
    endDate = new Date(dueDate)
    giorni = []
    while (startDate < dueDate) {
        if (startDate < end && startDate > start) giorni.push(startDate);
        startDate.setDate(startDate.getDate() + 1);
    }
    return giorni;

}
/*==================Crea Tabella========================= */
function inserisciTab() {
    /*'<div><div class="data"><div class="name"></div><div class="information"></div></div></div>'*/
    tab = document.getElementsByClassName("third")[0].innerHTML = ""
    let string = ""
    for (i = 0; i < listaTab1.length; i++) {
        string += '<div><div class="data"><div class="name">' + listaTab1[i][0] + '</div><div class="information">' + listaTab1[i][1] + '</div></div></div>'
    }
    document.getElementsByClassName("third")[0].innerHTML = string;
    document.getElementsByClassName("row")[0].style = "grid-template-rows: repeat(" + listaTab1.length + ", 1fr);";
}

function inseriscidiv(giorni) {
    document.getElementsByClassName("third_")[0].innerHTML = "";
    let string = "";
    for (i = 0; i < listaTab1.length * giorni; i++) {
        string += '<div></div>';
    }
    document.getElementsByClassName("third_")[0].innerHTML = string;
}

function inseriscigiorni(giorni) {
    document.getElementsByClassName("project")[0].innerHTML = "";
    let string = "";
    oggi = new Date();
    fine = new Date();
    fine.setDate(oggi.getDate() + giorni)
    for (i = oggi, j= 0; j < giorni; i.setDate(i.getDate() + 1), j++) {
        string += '<div><div>' + i.getDate() + '</div></div>'
    }
    document.getElementsByClassName("project")[0].innerHTML = string
    creaGiorni(oggi, fine);
}

function handleSelection() {
    var giorni = document.getElementById("days").value;
    document.getElementsByClassName("grid")[0].style = "grid-template-columns: repeat(" + giorni + ", 1fr);";
    document.getElementsByClassName("grid")[1].style = "grid-template-columns: repeat(" + giorni + ", 1fr); grid-template-rows: repeat(" + listaTab1.length + ", 1fr);";
    inseriscidiv(giorni);
    inseriscigiorni(giorni);
}
