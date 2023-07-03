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
class Contenitore {
    constructor(nome, array) {
        this.nome = nome
        this.array = array
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
        selectable: true,
        rowFormatter: function (columns) {
            if (columns.getData().style == "false") {
                columns.getElement().style.backgroundColor = "#720026";
                columns.getElement().style.color = "white";
            }
        },
        height: "311px",
        columns: [
            { title: "ID", field: "idReadable" },
            { title: "Responsabile", field: "fields.assignee" },
            { title: "Titolo", field: "summary" },
            { title: "StartDate", field: "fields.startDate" },
            { title: "EndDate", field: "fields.dueDate" },
            { title: "WorkEffort", field: "fields.workEffort" },
            { title: "Error", field: "style", formatter: "tickCross" },
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
    for (i = 0; i < data.length; i++) {
        if (data[i].fields.startDate > data[i].fields.dueDate) {
            data[i].style = "false";
        }
        else {
            data[i].style = "true";
        }
        if (data[i].fields.assignee == "" || data[i].fields.assignee == null) data[i].style = "false";
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
function getAvg(start, end, minutes) { //calcola il workEfford per giorno
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
function checkFestivo(data) {  //controllo weekend
    data = new Date(data);
    giornoSettimana = data.getDay();
    if (giornoSettimana == 0 || giornoSettimana == 6) {   //0 = domenica, 6 = sabato
        return false;
    } else { return true; }
}

function creaGiorni(inizio, fine) {
    const g = 2
    oreGiornata = []
    for (i = 0; i < listaTab1.length; i++) {

        for (j = 0; j < listaTab1[i][2].length; j++) {
            oreGiornata = dataCorretta(inizio, fine, listaTab1[i][g][j].startDate, listaTab1[i][g][j].dueDate, listaTab1[i][0]);
        }
    }


    // somma dei ticket giornalieri di ogni persona
    for(let i = 0; i<oreGiornata.length; i++){
        for(let j = 0; j<oreGiornata.length; j++){
            if(oreGiornata[i].nome == oreGiornata[j].nome){
                let mediaGiornaliera = 0
            }
        }
    }


}
function dailyAvg(array){
    for(let i = 0; i<array.length; i++){
        for(let j = 0; j<array.length; j++){

        }
    }
}
//            inizio tabella, fine tabella, start ticket, end ticket, nome
function dataCorretta(inizio, fine, startDate, dueDate, proprietario) {
    contenitore = []
    giorni = []
    workingDay = new Date(startDate)
    endDate = new Date(dueDate)
    while (workingDay <= fine) {
        if (workingDay > inizio) {
            push = new Date()
            push.setDate(workingDay.getDate() - 1)
            giorni.push(push)
        }
        workingDay.setDate(workingDay.getDate() + 1)
    }
    checkFormat(giorni, inizio)
    contenitore.push(new Contenitore(proprietario, giorni))
    return contenitore;

}
//array di giorni visualizzati, inizio tabella
function checkFormat(giorni, start) {
    start = new Date(start)
    phase = 0
    // L'array di giorni DEVE essere 7
    for (p = 0; p < 7; p++) {
        if (giorni.length != 7) {
            if (giorni[p] != start.getDate()) phase++
        }
        else return
    }
    for (p = 0; p < phase; p++) {
        giorni.unshift(0)
    }
    while (giorni.length != 7) {
        giorni.push(0)
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
    document.getElementsByClassName("third")[0].innerHTML = string;
    document.getElementsByClassName("row")[0].style = "grid-template-rows: repeat(" + listaTab1.length + ", 1fr);";
}

function inseriscidiv(giorni) {
    document.getElementsByClassName("third_")[0].innerHTML = "";
    let string = "";
    var dataOdierna = new Date();
    for (i = 0; i < listaTab1.length; i++) {
        for (j = 0; j < giorni; j++) {
            if (checkFestivo(dataOdierna)) {
                color = "white";
                uno = Math.random() * (90 - 0 + 1) + 0;
                num = Math.floor(uno) / 10;
                if (num == 0) {
                    num = "";
                } else if (num > 8) {
                    color = "darkred";
                }
                else if (num > 0 && num <= 3) {
                    color = "green";
                }
                else if (num > 3 && num <= 6) {
                    color = "yellow"
                }
                else if (num > 6 && num <= 8) {
                    color = "orange"
                }
                string += '<div style=" background-color: ' + color + '; text-align: center">' + num + '</div>';
            }
            else {
                string += '<div style=" background-color: #9c9c9c; text-align: center"></div>';
            }
            dataOdierna.setDate(dataOdierna.getDate() + 1);
        }
        dataOdierna = new Date(); 2
    }

    document.getElementsByClassName("third_")[0].innerHTML = string;
}

function inseriscigiorni(giorni) {
    document.getElementsByClassName("project")[0].innerHTML = "";
    let string = "";
    oggi = new Date();
    fine = new Date();
    fine.setDate(oggi.getDate() + parseInt(giorni))
    for (i = oggi, j = 0; j < giorni; i.setDate(i.getDate() + 1), j++) {
        string += '<div><div>' + i.getDate() + '</div></div>'
    }
    document.getElementsByClassName("project")[0].innerHTML = string
    creaGiorni(new Date(), fine);
}

function handleSelection() {
    var giorni = document.getElementById("days").value;
    document.getElementsByClassName("grid")[0].style = "grid-template-columns: repeat(" + giorni + ", 1fr);";
    document.getElementsByClassName("grid")[1].style = "grid-template-columns: repeat(" + giorni + ", 1fr); grid-template-rows: repeat(" + listaTab1.length + ", 1fr);";
    inseriscidiv(giorni);
    inseriscigiorni(giorni);
}
