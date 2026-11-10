// Executed in a separate LibreSprite process; never touches the user's open document.
var pc=app.pixelColor;
for(var i=0;i<plans.length;i++){
    var p=plans[i];app.open(p.source);var sprite=app.activeSprite;
    if(!sprite || sprite.width!==p.width || sprite.height!==p.height || sprite.filename.replace(/\\/g,'/')!==p.source)
        throw new Error('Wrong candidate document');
    var img=sprite.layer(0).cel(0).image;
    for(var j=0;j<p.changes.length;j++){
        var c=p.changes[j];img.putPixel(c[0],c[1],pc.rgba(c[2],c[3],c[4],c[5]));
    }
    sprite.commit();sprite.saveAs(dest+'Editable/'+p.key+'_Sheet.ase',true);
    sprite.saveAs(dest+'Cleaned/'+p.key+'_Sheet.png',true);app.activeDocument.close();
}
console.log('SILVER_RELIC_CLEANUP_COMPLETE');app.command.Exit();
