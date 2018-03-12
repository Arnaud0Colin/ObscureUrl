# ObscureUrl

## Option 

```
   /* Type of Shift on Encode Key for example Day the same string get same encoding for today */ 
   /* with Hour => encoded string get a different result every hour */ 
   Shift
      0 = Millisecond
      1 = Second
      2 = Minute
      3 = Hour
      5 = Day
      6 = Month
      7 = Year
default = Day  
```

```C#
  public Obscure(/* Encode Key */ byte[] Key)
  public Obscure(/* Encode Key */ byte[] Key, /* Add TimeStamp */ bool TimeStamp = false) 
  public Obscure(/* Encode Key */ byte[] Key, /* Add x Dump Byte */ int Complement = 0, /* Add TimeStamp */ bool TimeStamp = false) 
```

## Sample
```C#
// Encode Key
byte[] _XorKey = { 226, 245, 107, 211, 54, 158, 173, 18, 171, 24, 234, 236, 53, 154, 136, 24, 70, 35, 178, 102, 76, 9,
                  119, 152, 225, 150, 179, 38, 17, 109, 213, 87, 81, 177, 127, 6, 24, 22, 177, 95, 184, 143, 234, 239, 
                  35, 5, 116, 76, 71, 114, 211, 54, 17, 161, 71, 164, 195, 211, 106, 6, 37, 202, 24, 29, 58, 211, 121, 
                  117, 87, 24, 170, 108, 154, 64, 226, 127, 13, 244, 13, 49, 175, 108, 129, 127, 111, 62, 252, 29, 131, 
                  208, 142, 168, 34, 184, 62, 150, 180, 127, 74, 76, 226, 245, 107, 211, 54, 158, 173, 18, 171, 24, 234, 
                  236, 53, 154, 136, 24, 70, 35, 178, 102, 76, 9, 119, 152, 225, 150, 179, 38, 17, 109, 213, 87, 81, 177, 
                  127, 6, 24, 22, 177, 95, 184, 143, 234, 239, 35, 5, 116, 76, 71, 114, 211, 54, 17, 161, 71, 164, 195, 211, 
                  106, 6, 37, 202, 24, 29, 58, 211, 121, 117, 87, 24, 170, 108, 154, 64, 226, 127, 13, 244, 13, 49, 175, 108, 
                  129, 127, 111, 62, 252, 29, 131, 208, 142, 168, 34, 184, 62, 150, 180, 127, 74, 76, 226, 245, 107, 211, 54, 
                  158, 173, 18, 171, 24, 234, 236, 53, 154, 136, 24, 70, 35, 178, 102, 76, 9, 119, 152, 225, 150, 179, 38, 17, 
                  109, 213, 87, 81, 177, 127, 6, 24, 22, 177, 95, 184, 143, 234, 239, 35, 5, 116, 76, 71, 114, 211, 54, 17, 161, 
                  71, 164, 195, 211, 106, 6, 37, 202, 24, 29, 58, 211, 121, 117, 87, 24, 170, 108, 154, 64, 226, 127, 13, 244, 
                  13, 49, 175, 108, 129, 127, 111, 62, 252, 29, 131, 208, 142, 168, 34, 184, 62, 150, 180, 127, 74, 76 };

string Chaine = "Good Morning";

   var ObsChaine = new SecurityExtention.Obscure(_XorKey).Encoder(Chaine, SecurityExtention.ObscureStringMode.ASCII); 
   //  ObsChaine == "DMguX3cm-9C80ck-mPEs"
   
   string Clear = new SecurityExtention.Obscure(_XorKey).Dec_String(ObsChaine, SecurityExtention.ObscureStringMode.ASCII);
  // Clear == "Good Morning"

```


## Sample for a Url
```c#
 // For Url like /tree/master?readme=1
 string Chaine = "readme=1";
 var ObsChaine = new SecurityExtention.Obscure(_XorKey).Encoder(Chaine, SecurityExtention.ObscureStringMode.ASCII);
 //  Get url like /tree/master?DJ4rVn066erUIpM,
```            
  * Obscure(_XorKey)            => "DJ4rVn066erUIpM,"
  * Obscure(_XorKey){Shift=2}   => "OCF2FV3zivK6lmc,"   /* AT 2:56 pm */
  * Obscure(_XorKey){Shift=2}   => "OYiz81QoVAuqLAo,"   /* AT 2:57 pm */
  * Obscure(_XorKey, 1, false ) => "9QyeK1Z9Ounq1CKT"
  * Obscure(_XorKey, 5, false ) => "9WvTNp4MnitWfTrp6tQikw,,"
  * Obscure(_XorKey, 0, true  ) => "DJ4rVn066erU1Xdnr3CHLCqwIQ,,"
   
   All Can be decrypt and return "readme=1"
   
   
