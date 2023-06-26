let dati;
$(document).ready(async function () {
  dati = await leggiticket();
  aggiornaTabella();
});



function aggiornaTabella() {
  
    

  /*var table = new Tabulator("#projectTable", {
    height:"311px",
    columns:[
    {title:"ID", field:"idReadable"},
    {title:"Responsabile", field:"fields.assignee"},
    {title:"Titolo", field:"summary"},
    {title:"StartDate", field:"fields.startDate"},
    {title:"EndDate", field:"fields.endDate"},
    {title:"WorkEffort", field:"fields.workEffort"},
    ],
  });
   table.setData(data)*/


 
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
  
  /*
  j = 0;
  let projectTable = $("#projectTable");
  projectTable.innerHTML = "";
  let tableBody = $("#tableBody");
  tableBody.empty();

  for (i = 0; i < data.length; i++) {
    row = document.createElement("tr");

    idCell = document.createElement("td");
    idCell.innerHTML = data[i].ID;
    row.appendChild(idCell);

    responsabileCell = document.createElement("td");
    responsabileCell.innerHTML = data[i].Responsabile;
    row.appendChild(responsabileCell);

    titoloCell = document.createElement("td");
    titoloCell.innerHTML = data[i].Titolo;
    row.appendChild(titoloCell);

    startDateCell = document.createElement("td");
    startDateCell.innerHTML = data[i].StartDate;
    row.appendChild(startDateCell);

    endDateCell = document.createElement("td");
    endDateCell.innerHTML = data[i].EndDate;
    row.appendChild(endDateCell);

    workEffortCell = document.createElement("td");
    workEffortCell.innerHTML = data[i].WorkEffort;
    row.appendChild(workEffortCell);

    tableBody.append(row);
  }
  */
}

async function leggiticket() {
  let file = 'https://localhost:7075/Dati';
  const response = await fetch(file);
  data = await response.json();

  return data;
}

/*
function ordinaDati(campo) {
  data = data.sort(function (a, b) {
    if (a[campo] < b[campo])
      return -1;
    if (a[campo] > b[campo])
      return 1;

    return 0;
  });
  aggiornaTabella();
}
*/