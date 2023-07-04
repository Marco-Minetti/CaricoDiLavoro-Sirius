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
//modificare nome
class Pezzi {
    constructor(id, prio, dueDate, startDate, workEffort, state, avg) {
        this.id = id
        this.prio = prio
        this.dueDate = dueDate
        this.startDate = startDate
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
                console.log(getAvg(data[j].fields.startDate, data[j].fields.dueDate, data[j].fields.workEffort), data[j].idReadable)
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
/*==================Calcoli per la tabella=========================*/


function calculateTicketDuration(data, startDate, endDate) {
    const result = [];

    let currentAuthor = null;
    let authorTickets = [];
    let inizio = new Date();
    let fine = new Date();
    fine = fine.setDate(fine.getDate() + 7);

    for (var item of data) {
        if (Array.isArray(item)) {
            if (currentAuthor !== null) {
                const authorIndex = result.findIndex((entry) => entry.author === currentAuthor);
                if (authorIndex !== -1) {
                    result[authorIndex].duration = calculateAuthorTicketDuration(authorTickets, startDate, endDate, inizio, fine);
                } else {
                    result.push({ author: currentAuthor, duration: calculateAuthorTicketDuration(authorTickets, startDate, endDate, inizio, fine) });
                }
                authorTickets = [];
            }

            currentAuthor = item[0];
            authorTickets = item[2];
        }
    }

    if (currentAuthor !== null) {
        const authorIndex = result.findIndex((entry) => entry.author === currentAuthor);
        if (authorIndex !== -1) {
            result[authorIndex].duration = calculateAuthorTicketDuration(authorTickets, startDate, endDate, inizio, fine);
        } else {
            result.push({ author: currentAuthor, duration: calculateAuthorTicketDuration(authorTickets, startDate, endDate, inizio, fine) });
        }
    }

    return result;
}

function calculateAuthorTicketDuration(tickets, startDate, endDate, filterStart, filterEnd) {
    var result = {};
    const fine = new Date(filterEnd)
    filterStart.setHours(11, 0, 0) // potrebbe dare problemi
    for (var ticket of tickets) {
        const ticketStartDate = new Date(ticket.startDate);
        const ticketEndDate = new Date(ticket.dueDate);

        if (ticketStartDate <= endDate && ticketEndDate >= startDate) {
            days = Math.ceil((ticketEndDate - ticketStartDate) / (1000 * 60 * 60 * 24));

            for (let i = 0; i < days; i++) {
                currentDate = new Date(ticketStartDate);
                currentDate.setDate(ticketStartDate.getDate() + i);
                currentDateISO = currentDate.toISOString().split('T')[0];

                nome = ticket.id
                avgValue = ticket.avg

                if (currentDate >= filterStart && currentDate <= fine) {
                    if (!result[currentDateISO]) {
                        result[currentDateISO] = {
                            sum: 0,
                        };
                    }

                    result[currentDateISO].sum += avgValue;
                }
            }
        }
    }

    for (var dateISO in result) {
        result[dateISO] = result[dateISO].sum.toFixed(2);
    }
    return result;
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

    fine = new Date()
    fine = fine.setDate(fine.getDate() + 7)
    strutturaFinale = calculateTicketDuration(listaTab1, new Date(), fine)

    for (var ticket of strutturaFinale) {
        for (let j = new Date(); j < fine; j.setDate(j.getDate() + 1)) {
            ISOoggi = j.toISOString().split('T')[0]
            if (checkFestivo(j)) {
                color = "white"
                num = ticket.duration[ISOoggi]
                
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

                
                if (num != undefined) {
                    string += '<div style=" background-color: ' + color + '; text-align: center">' + num + '</div>'
                } else string += '<div style=" background-color: ' + color + '; text-align: center">' + 0 + '</div>'

            } else string += '<div style=" background-color: #9c9c9c; text-align: center"></div>';
        }
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
}

function handleSelection() {
    var giorni = document.getElementById("days").value;
    document.getElementsByClassName("grid")[0].style = "grid-template-columns: repeat(" + giorni + ", 1fr);";
    document.getElementsByClassName("grid")[1].style = "grid-template-columns: repeat(" + giorni + ", 1fr); grid-template-rows: repeat(" + listaTab1.length + ", 1fr);";
    inseriscidiv(giorni);
    inseriscigiorni(giorni);
}