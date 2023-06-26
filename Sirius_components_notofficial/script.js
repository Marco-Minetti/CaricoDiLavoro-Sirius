/* limita i caratteri dopo il 20

var e=document.querySelectorAll('option')
e.forEach(x=>{
if(x.textContent.length>20)
x.textContent=x.textContent.substring(0,20)+'...';
}) */

$(document).ready(async function () {
    dati = await leggiticket();
    aggiornaTabella();
    mostraOrarioPreciso();
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

  
  
  let dati;

function aggiornaTabella() {

  var table = new Tabulator("#projectTable", {
    data:dati,
    height:"311px",
    columns:[
    {title:"ID", field:"idReadable"},
    {title:"Responsabile", field:"fields.assignee"},
    {title:"Titolo", field:"summary"},
    {title:"StartDate", field:"fields.startDate"},
    {title:"EndDate", field:"fields.dueDate"},
    {title:"WorkEffort", field:"fields.workEffort"},
    ],
});

}

async function leggiticket() {
    let file = 'https://localhost:7075/Dati';
    const response = await fetch(file);
    data = await response.json();
  
  

  return data;
}





