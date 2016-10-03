# dmr
Dansk motor register registreringsnummer lookup scraper.

Dmr består af en scaper der indhenter data, og en console app der kan vise resultatet.

    var licencePlate = "SOME REG NR."
    var model = Scraper.LookupVehicle(licencePlate);
    
    // get result
    var token = model.Token;
    var json = model.ToJson();
