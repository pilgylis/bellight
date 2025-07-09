using System.Security.Cryptography;
using EntityFrameworkTests.Models;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkTests;

public class TestingDbContext(DbContextOptions<TestingDbContext> options) : DbContext(options)
{
    public DbSet<Counter> Counters { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Counter>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("int")
                .IsRequired();

            entity.Property(e => e.Value)
                .HasColumnName("value")
                .HasColumnType("int");

            entity.Property(e => e.IsDeleted)
                .HasColumnName("isDeleted");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("int")
                .IsRequired();

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasColumnType("varchar(255)");

            entity.Property(e => e.Email)
                .HasColumnName("email")
                .HasColumnType("varchar(255)");

            entity.Property(e => e.IsDeleted)
                .HasColumnName("isDeleted");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("int")
                .IsRequired();

            entity.Property(e => e.CustomerId)
                .HasColumnName("customer_id")
                .HasColumnType("int")
                .IsRequired();

            entity.OwnsMany(e => e.Products, ownedNavigationBuilder =>
            {
                ownedNavigationBuilder.ToJson();
            });

            entity.Property(e => e.DiscountAmount)
                .HasColumnName("discount_amount")
                .HasColumnType("money");

            entity.Property(e => e.Total)
                .HasColumnName("total")
                .HasColumnType("money");

            entity.Property(e => e.IsDeleted)
                .HasColumnName("isDeleted");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("int")
                .IsRequired();

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasColumnType("varchar(255)");

            entity.Property(e => e.Price)
                .HasColumnName("price")
                .HasColumnType("money");

            entity.Property(e => e.IsDeleted)
                .HasColumnName("isDeleted");
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseAsyncSeeding(async (context, _, cancellationToken) =>
        {
            await AddCustomers(context);
            await AddProducts(context);
            await AddOrders(context);
        });

    private static async Task AddCustomers(DbContext context)
    {
        // import customer
        var customerSet = context.Set<Customer>();
        var randomCustomer = await customerSet.FirstOrDefaultAsync();
        if (randomCustomer is not null)
        {
            return;
        }

        #region Customer Data

        var customerData = @"Kimmy Pachta,kpachta0@shareasale.com
            Claybourne Bance, cbance1@unblog.fr
            Matti Portch,mportch2 @merriam-webster.com
            Rodrique Oxenden, roxenden3@google.pl
            Beniamino Derle,bderle4 @nytimes.com
            Glyn Andrejevic,gandrejevic5 @etsy.com
            Jackie Haymes,jhaymes6 @slideshare.net
            Selle Gammill,sgammill7 @comsenz.com
            Trstram Bolding,tbolding8 @alexa.com
            Linus Napton,lnapton9 @gmpg.org";

        #endregion Customer Data

        foreach (var line in customerData.Split('\r'))
        {
            var lineTrimmed = line.Trim();
            if (string.IsNullOrEmpty(lineTrimmed))
            {
                continue;
            }

            var parts = lineTrimmed.Split(',');
            if (parts.Length != 2)
            {
                continue;
            }

            var customer = new Customer
            {
                Name = parts[0].Trim(),
                Email = parts[1].Trim().Replace(" ", ""),
            };

            await customerSet.AddAsync(customer);
        }

        await context.SaveChangesAsync();
    }

    private async Task AddProducts(DbContext context)
    {
        var productSet = context.Set<Product>();
        var randomProduct = await productSet.FirstOrDefaultAsync();
        if (randomProduct is not null)
        {
            return;
        }

        #region Product Data

        var productData = @"Eel Fresh,5948282
            Oil - Shortening - All - Purpose,2698362
            Bread - White Epi Baguette,3600146
            Red Snapper - Fillet, Skin On,9911140
            Rice - Aborio,4816387
            Thyme - Fresh,2488477
            Flour - Bran, Red,9527527
            Fruit Mix - Light,4598119
            Peas - Pigeon, Dry,2280197
            Flour - Buckwheat, Dark,9142817
            Oil - Truffle, Black,8801984
            Turkey - Breast, Smoked,2633815
            Wine - Guy Sage Touraine,2214832
            Wine - Rosso Toscano Igt,6236193
            Cookies - Englishbay Chochip,2971605
            Drambuie,5782887
            Sausage - Chorizo,3748857
            Tomatoes Tear Drop Yellow,1607788
            Sprouts - Bean,2705877
            Foil Cont Round,1620132
            Relish,3365138
            Lemonade - Kiwi, 591 Ml,356207
            Melon - Watermelon, Seedless,8134694
            Tray - 16in Rnd Blk,5638843
            Stainless Steel Cleaner Vision,6709303
            Tart Shells - Sweet, 3,7586692
            Bread Cranberry Foccacia,9712968
            Beer - Upper Canada Light,2074787
            Shrimp - Black Tiger 16/20,2763854
            Vermouth - White, Cinzano,6238458
            Milk - 2%,1369730
            Sugar - Palm,9415986
            Cut Wakame - Hanawakaba,8663268
            Bread - Bistro White,9810231
            Kohlrabi,2108804
            Pasta - Lasagna Noodle, Frozen,6973115
            Towel Dispenser,3750163
            Wine - White, Gewurtzraminer,4463388
            Swordfish Loin Portions,2189382
            Soup - Campbells, Classic Chix,2664766
            Soup - Beef Conomme, Dry,7230717
            Olives - Nicoise,3089136
            Clams - Canned,6208506
            Lettuce - California Mix,4716749
            Soup - French Onion,6296557
            Pepper - Scotch Bonnet,1100392
            Flower - Leather Leaf Fern,9265216
            Nori Sea Weed,3056122
            Bread - Dark Rye,2409618
            Seedlings - Mix, Organic,2400135
            Crab - Soft Shell,1627867
            Strawberries,8616077
            Pasta - Linguini, Dry,5135035
            Wine - Rhine Riesling Wolf Blass,8063883
            Sugar - White Packet,7941995
            Pork Salted Bellies,1969040
            Blueberries - Frozen,6258019
            Hummus - Spread,1509006
            Tuna - Canned, Flaked, Light,1056369
            Dip - Tapenade,7418037
            Pastry - Raisin Muffin - Mini,1661731
            Beans - Turtle, Black, Dry,3039727
            Hot Chocolate - Individual,7697486
            Ice - Clear, 300 Lb For Carving,4100883
            Beef Cheek Fresh,1352554
            Pork - Loin, Bone - In,928973
            Grapefruit - Pink,9894673
            Lettuce - Lambs Mash,6986945
            Veal - Kidney,6805034
            Table Cloth - 53x69 Colour,3255753
            Mountain Dew,4740239
            Alize Gold Passion,2424227
            Passion Fruit,4647531
            Towel Multifold,1426540
            Wine - German Riesling,6660584
            Hersey Shakes,1359562
            Venison - Ground,2386011
            Spice - Montreal Steak Spice,4436757
            Foam Cup 6 Oz,4596465
            Tomatoes Tear Drop,7812952
            Jerusalem Artichoke,9192086
            Myers Planters Punch,9412068
            Lentils - Green, Dry,2590888
            Lamb - Loin, Trimmed, Boneless,7967210
            Watercress,1843813
            V8 Splash Strawberry Kiwi,2140588
            Aspic - Light,9437187
            Cake Circle, Foil, Scallop,5222123
            Irish Cream - Butterscotch,2863848
            Potatoes - Idaho 100 Count,2084466
            Beef - Sushi Flat Iron Steak,7984396
            Wine - Two Oceans Cabernet,8671388
            Vinegar - Rice,9940399
            Soup Campbells Split Pea And Ham,8176369
            Turnip - Mini,5863732
            Pork - Hock And Feet Attached,8292917
            Veal - Provimi Inside,7732754
            Mousse - Mango,3400925
            Soup - Campbells - Tomato,9155523
            Broom - Push,3293067
            Bread - Ciabatta Buns,8463606
            Chef Hat 20cm,495065
            Sprouts - Alfalfa,9632891
            Nut - Pistachio, Shelled,8148304
            Bread - White, Unsliced,1812719
            Hot Chocolate - Individual,6689128
            Herb Du Provence - Primerba,3683633
            Onions - Dried, Chopped,6125398
            Appetiser - Bought,5260336
            Gin - Gilbeys London, Dry,171323
            Frangelico,2019674
            Wine - Red, Mosaic Zweigelt,9061042
            Quail - Jumbo,7699535
            Beef Striploin Aaa,2536420
            Scotch - Queen Anne,6504492
            Parsley - Dried,2867010
            Wine - Zinfandel Rosenblum,1618587
            Muffin Batt - Ban Dream Zero,4417930
            Mince Meat - Filling,3955891
            Apple - Delicious, Red,3727204
            Bag Stand,7627960
            Fruit Mix - Light,9320114
            Spice - Chili Powder Mexican,2145688
            Sugar - Brown, Individual,9641527
            Chicken - Leg, Boneless,3477327
            Cheese - Mozzarella, Buffalo,2870957
            Pasta - Lasagna, Dry,1108724
            Pasta - Elbows, Macaroni, Dry,7781383
            Wine - Redchard Merritt,6066567
            Chicken - Whole Fryers,2073110
            Croissants Thaw And Serve,347618
            Table Cloth 62x120 White,3412041
            Ketchup - Tomato,6427228
            Kellogs Raisan Bran Bars,1325665
            Chicken Giblets,6721494
            Turkey - Breast, Boneless Sk On,9022113
            Pasta - Spaghetti, Dry,3256433
            Lettuce - Frisee,7296650
            Bacardi Limon,518844
            Pepper - Gypsy Pepper,5542670
            Sun - Dried Tomatoes,5619689
            Soup - Verve - Chipotle Chicken,5842463
            Longos - Greek Salad,4885380
            Turkey - Breast, Smoked,4426420
            Veal - Slab Bacon,8970703
            Caviar - Salmon,5360744
            Jam - Blackberry, 20 Ml Jar,3476368
            Veal - Round, Eye Of,3954820
            Crab - Dungeness, Whole,3233170
            Shrimp - 16 - 20 Cooked, Peeled,3259892
            Mushroom - Chanterelle, Dry,4996581
            Corn Syrup,408333
            Pie Box - Cello Window 2.5,1489958
            Miso - Soy Bean Paste,7017327
            Wine - Chateauneuf Du Pape,3091830
            Syrup - Chocolate,5169815
            Beer - Original Organic Lager,585993
            Cheese - Brie, Triple Creme,9820526
            Soho Lychee Liqueur,1137485
            Pasta - Gnocchi, Potato,5250224
            Arctic Char - Fresh, Whole,2188235
            Soup Knorr Chili With Beans,8278700
            Carroway Seed,2559010
            Beer - Upper Canada Lager,4682498
            Flower - Leather Leaf Fern,1258915
            Crab - Claws, 26 - 30,5383925
            Bread - Pumpernickle, Rounds,5053714
            Clams - Bay,9440862
            Bagelers - Cinn / Brown,5797870
            Beer - Pilsner Urquell,5463254
            Water - Mineral, Carbonated,1386148
            Mace Ground,8447690
            Garlic,9588183
            Salt And Pepper Mix - Black,1942488
            Soup - Campbells Mac N Cheese,1732449
            Petite Baguette,8865199
            Tart Shells - Sweet, 3,2633095
            Club Soda - Schweppes, 355 Ml,207190
            Snapple Raspberry Tea,5592121
            Pork - European Side Bacon,8957391
            Bagels Poppyseed,5789688
            Juice - Happy Planet,8591395
            Rum - Light, Captain Morgan,9342202
            Iced Tea - Lemon, 340ml,8337265
            Wine - Taylors Reserve,8926957
            Cheese Cloth No 60,3810725
            Chutney Sauce - Mango,530862
            Milk - Chocolate 500ml,4014293
            Lambcasing,5689671
            Chocolate - Semi Sweet,8462299
            Smoked Tongue,4185843
            Chilli Paste, Hot Sambal Oelek,796348
            Yogurt - Peach, 175 Gr,613628
            Flour - Strong Pizza,4049728
            Devonshire Cream,4344628
            Anchovy Fillets,8019597
            Salad Dressing,9071463
            Muffin Mix - Morning Glory,2815781
            Raspberries - Fresh,4258545
            Wine - Balbach Riverside,4781797
            Pineapple - Regular,2785634
            Wiberg Cure,151168
            Table Cloth 62x120 Colour,331887
            Garlic - Primerba, Paste,473582
            Flavouring - Orange,6613090
            Basil - Seedlings Cookstown,4441060
            Flower - Potmums,2999081
            Veal - Insides Provini,750827
            Coffee Decaf Colombian,4767474
            Tea - Mint,1421260
            Cake - Pancake,2488602
            Ecolab Crystal Fusion,6424060
            Trueblue - Blueberry,3132880
            Chocolate - Milk, Callets,1232431
            Scallops 60/80 Iqf,6682084
            Veal - Ground,582424
            Lamb - Whole, Fresh,3363097
            Wine - Saint Emilion Calvet,6798397
            Wine - White, Ej,398702
            Chocolate - Semi Sweet, Calets,7160062
            Wine - Cahors Ac 2000, Clos,8763803
            Peas - Frozen,2616308
            Kolrabi,531695
            Quail - Whole, Boneless,7055683
            Coke - Classic, 355 Ml,2920596
            Wine - Valpolicella Masi,8115128
            Jagermeister,2852885
            Extract - Raspberry,1794105
            Beer - Alexander Kieths, Pale Ale,9232304
            Wakami Seaweed,9058426
            Pickles - Gherkins,8627324
            Soup V8 Roasted Red Pepper,8982113
            Mix - Cocktail Ice Cream,6874937
            Soda Water - Club Soda, 355 Ml,9066355
            Soup - Campbells, Cream Of,2480066
            Tomatoes - Orange,8024643
            Cakes Assorted,5947978
            Stock - Veal, White,5758720
            Tea - Darjeeling, Azzura,9522638
            Coffee - 10oz Cup 92961,7832346
            Roe - Flying Fish,7876115
            Sesame Seed,3167231
            Cleaner - Pine Sol,5882591
            Soup - Cream Of Broccoli, Dry,6305213
            Lemonade - Kiwi, 591 Ml,102194
            Rum - Light, Captain Morgan,3754179
            Lemonade - Kiwi, 591 Ml,4993739
            Chocolate - Pistoles, Lactee, Milk,4046047
            Hersey Shakes,6049219
            Sobe - Tropical Energy,4949066
            Sherry - Dry,4572371
            Pork - European Side Bacon,7522549
            Watercress,5602980
            Salmon - Sockeye Raw,9709741
            Ecolab - Orange Frc, Cleaner,1529833
            Sausage - Breakfast,699871
            Pumpkin - Seed,5735116
            Pie Shell - 5,1979705
            Pepper - Cubanelle,3463432
            Muffin - Mix - Mango Sour Cherry,9650986
            Pork - Hock And Feet Attached,1360334
            Sandwich Wrap,5799551
            Tart Shells - Sweet, 2,5517502
            Wine - Cabernet Sauvignon,1169236
            Wine - Rubyport,7483585
            Coffee - Irish Cream,2863178
            Anchovy Fillets,7412006
            Grapes - Red,8686875
            Yams,8717918
            Dill Weed - Dry,5660655
            Bread - Sticks, Thin, Plain,2089445
            Cloves - Ground,285189
            Tea - Herbal Orange Spice,4599157
            Ice Cream Bar - Hageen Daz To,5193797
            Dill - Primerba, Paste,3171395
            Wine - Stoneliegh Sauvignon,9637120
            Salt - Kosher,2003994
            Wine - Champagne Brut Veuve,1631459
            Jam - Raspberry,jar,6425761
            Ecolab - Hobart Washarm End Cap,3336606
            Salt - Kosher,5978191
            Wine - Magnotta - Belpaese,1107164
            Ocean Spray - Ruby Red,4297873
            Gatorade - Lemon Lime,4003926
            Salsify, Organic,4038539
            Beer - Camerons Auburn,5503087
            Soup - Knorr, Ministrone,5970990
            Oregano - Fresh,491613
            Pepper - Paprika, Hungarian,3011962
            Beef - Top Butt,330935
            Brownies - Two Bite, Chocolate,4289542
            Cheese - Parmesan Cubes,1244232
            Shrimp - Black Tiger 8 - 12,3454221
            Wine - Baron De Rothschild,8184553
            Soup Campbells Split Pea And Ham,4452896
            Langers - Ruby Red Grapfruit,1325814
            Sugar - Sweet N Low, Individual,6364286
            Muffin Batt - Choc Chk,8628511
            Brandy Cherry - Mcguinness,8083266
            Whmis - Spray Bottle Trigger,3308843
            Cheese - Brie, Cups 125g,2770869
            Dc - Frozen Momji,2586921
            Puree - Kiwi,9406881
            Cream Of Tartar,3235817
            Oil - Sesame,2462437
            Mussels - Cultivated,3247136
            Grouper - Fresh,9709857
            Bols Melon Liqueur,1343801
            Olives - Stuffed,4897804
            Coffee - Egg Nog Capuccino,930423
            Wine - Acient Coast Caberne,7457522
            Oats Large Flake,8197427
            Bread - Roll, Calabrese,8783511
            Aspic - Light,8203944
            Oil - Shortening - All - Purpose,350326
            Wine - Champagne Brut Veuve,3572096
            Wine - Red, Cooking,8933432
            Carbonated Water - Lemon Lime,184520
            Tuna - Salad Premix,163390
            Cookies - Fortune,3300409
            Broom - Angled,3940981
            Veal - Inside Round / Top, Lean,971982
            Wine - Barossa Valley Estate,6969924
            Anchovy Fillets,4093559
            Soup Campbells Mexicali Tortilla,7643656
            Bagel - Whole White Sesame,3236387
            Sprite - 355 Ml,2502756
            Nut - Cashews, Whole, Raw,2386344
            Bread - Mini Hamburger Bun,1960220
            Tomato - Green,4125752
            Foam Cup 6 Oz,3636534
            Garam Masala Powder,4945073
            Cheese - Cheddarsliced,827034
            Table Cloth 54x72 Colour,2769632
            Garam Marsala,3204493
            Cheese - Gouda Smoked,9303058
            Skewers - Bamboo,2008057
            Zucchini - Yellow,5685310
            Blueberries - Frozen,1895601
            Yeast Dry - Fleischman,6575259
            Marzipan 50/50,3554984
            Bread - Roll, Italian,4675871
            Wine - White, Pinot Grigio,4829399
            Broom Handle,6259171
            Cheese - St. Paulin,7376704
            Pepper - Sorrano,6756949
            Trueblue - Blueberry Cranberry,3651309
            Doilies - 10, Paper,2732946
            Icecream - Dstk Cml And Fdg,6236056
            Island Oasis - Strawberry,1492248
            Onions - Cippolini,8230022
            Durian Fruit,6988057
            Rolled Oats,3281733
            Wine - Two Oceans Cabernet,980886
            Doilies - 7, Paper,8472419
            Soup - Knorr, Classic Can. Chili,6983273
            Beer - Molson Excel,3663997
            Truffle Paste,2256737
            Bulgar,5665169
            Pork - Bacon Cooked Slcd,1810579
            Skewers - Bamboo,5936594
            Table Cloth 53x69 White,5172464
            Coffee - Decaffeinato Coffee,3098459
            Island Oasis - Raspberry,6609025
            Cabbage - Savoy,7599512
            Wine - Magnotta - Pinot Gris Sr,997996
            Pepper - White, Ground,9350296
            Muffins - Assorted,5169838
            Tarts Assorted,6402185
            Bread - Olive,5499940
            Cheese - Perron Cheddar,8076818
            Kiwi,7333249
            Beef - Striploin Aa,8984849
            Wine - Wyndham Estate Bin 777,9007922
            Blue Curacao - Marie Brizard,1760668
            Jolt Cola,3910663
            Quail Eggs - Canned,3436359
            Juice - Lemon,6023478
            Chocolate - Semi Sweet, Calets,8544915
            Wine - Chablis 2003 Champs,4059449
            Cod - Fillets,284685
            Pepper - Black, Ground,7991662
            Mushroom - Shitake, Fresh,961783
            Pop Shoppe Cream Soda,4928879
            Soup - Knorr, Classic Can. Chili,9927387
            Pepper - Sorrano,7407950
            Coriander - Seed,9143760
            Glass Clear 7 Oz Xl,9126626
            Lettuce - Romaine, Heart,9589999
            Jam - Apricot,6720417
            Egg - Salad Premix,8716073
            Pineapple - Golden,2785941
            Beef - Sushi Flat Iron Steak,4930918
            Trueblue - Blueberry 12x473ml,9376587
            Nacho Chips,6956567
            Truffle - Peelings,9785519
            Pate - Cognac,6462377
            Salmon - Atlantic, Fresh, Whole,3524224
            Mussels - Frozen,3996842
            Cheese - Asiago,2083428
            Wine - Prem Select Charddonany,2184769
            Pasta - Fusili, Dry,6735837
            Grapefruit - White,3379197
            Coffee Cup 16oz Foam,5288748
            Fish - Base, Bouillion,2680296
            Milk - Chocolate 500ml,4099842
            Pastry - Chocolate Marble Tea,9736872
            Wine - Alicanca Vinho Verde,2518100
            Asparagus - White, Canned,982610
            Cookie Dough - Peanut Butter,7981099
            Coriander - Ground,5367187
            Apple - Delicious, Golden,8638309
            Wine - Red, Cooking,3712461
            Parsnip,9222941
            Flower - Carnations,3020076
            Chutney Sauce - Mango,8629030
            Cleaner - Pine Sol,411963
            Salmon - Fillets,864689
            Garam Marsala,4901226
            Wine - Masi Valpolocell,639916
            Melon - Honey Dew,1145009
            Artichoke - Bottom, Canned,7614446
            Mushroom - Morel Frozen,9796364
            Pike - Frozen Fillet,7620888
            Olives - Kalamata,184117
            Bread - 10 Grain,1438841
            Plate - Foam, Bread And Butter,9075521
            Butcher Twine 4r,7141900
            Peas Snow,934592
            Icecream Cone - Areo Chocolate,6099836
            Pasta - Spaghetti, Dry,541886
            Nut - Hazelnut, Whole,9844959
            Cattail Hearts,6957498
            Pie Box - Cello Window 2.5,5330445
            Cherries - Bing, Canned,3813919
            Scallops - Live In Shell,993868
            Pike - Frozen Fillet,7125606
            Hinge W Undercut,3912623
            Beer - Upper Canada Lager,3329710
            Muffin Carrot - Individual,5824255
            Tea - Lemon Green Tea,6050711
            Muffin - Mix - Mango Sour Cherry,8407408
            Bread Bowl Plain,3990184
            Longos - Chicken Wings,5369079
            Compound - Rum,6618599
            Emulsifier,1227481
            Chinese Lemon Pork,6323702
            Pizza Pizza Dough,6337619
            Chips - Potato Jalapeno,9679614
            Wine - Beringer Founders Estate,8744426
            Fish - Soup Base, Bouillon,8346051
            Wine - Clavet Saint Emilion,6838686
            Lamb - Leg, Boneless,3515168
            Nantucket Pine Orangebanana,1458527
            Longos - Cheese Tortellini,9619359
            Spice - Peppercorn Melange,2451656
            Longos - Assorted Sandwich,4023387
            Arctic Char - Fillets,6789240
            Hog / Sausage Casing - Pork,9485446
            Wine - Cahors Ac 2000, Clos,8897648
            Cup - Paper 10oz 92959,3521397
            Scallop - St. Jaques,2433345
            Cheese - Brie Roitelet,3984428
            Ham - Proscuitto,7457315
            Vodka - Smirnoff,6068717
            Nut - Almond, Blanched, Ground,6676118
            Macaroons - Two Bite Choc,3834401
            Knife Plastic - White,8948059
            Pork - Ground,2721661
            Pepper - Black, Crushed,1103837
            Nescafe - Frothy French Vanilla,7469197
            Beef - Eye Of Round,293756
            Wasabi Powder,2512966
            Chicken Breast Wing On,5354573
            Pepper - Chili Powder,590072
            Rum - Mount Gay Eclipes,852791
            Compound - Rum,6939625
            Wakami Seaweed,5849978
            Rye Special Old,103356
            Mangoes,8720853
            Bagelers - Cinn / Brown,6922008
            Pasta - Linguini, Dry,1057129
            Yogurt - Assorted Pack,267171
            Cheese - Gouda,8011095
            Cheese - Shred Cheddar / Mozza,2011618
            Chips - Potato Jalapeno,2835678
            French Pastries,8905217
            Food Colouring - Red,997919
            Baking Powder,9915219
            Pears - Fiorelle,3536916
            Asparagus - Mexican,6820165
            Salmon - Atlantic, Skin On,8392960
            Kiwano,9566558
            Kiwi Gold Zespri,6008856
            Chips Potato All Dressed - 43g,9442149
            Lemonade - Strawberry, 591 Ml,2003969
            Beans - Black Bean, Preserved,1025095
            Lettuce - Frisee,6249079
            Basil - Pesto Sauce,9316914
            Wine - Red, Antinori Santa,4051985
            Black Currants,3914210
            Bread - French Baquette,3177287
            Bread - Pumpernickle, Rounds,2360181
            Soup - Knorr, Classic Can. Chili,9904859
            Carbonated Water - Strawberry,6574109
            Soupfoamcont12oz 112con,5864665
            Yogurt - Assorted Pack,1226519
            Beef - Ox Tongue, Pickled,2186542
            Oxtail - Cut,6246524
            Triple Sec - Mcguinness,8239289
            Waffle Stix,998491
            Brocolinni - Gaylan, Chinese,7283582
            Jagermeister,2217278
            Spice - Peppercorn Melange,2284215
            Tomatoes - Plum, Canned,865226
            Lid Coffee Cup 8oz Blk,3490034
            Stock - Beef, White,1797722
            Pizza Pizza Dough,2135150
            Wine - White, Pinot Grigio,3525924
            Yeast Dry - Fermipan,9036322
            Cookies Cereal Nut,4708872
            Muffin - Zero Transfat,4563109
            Spice - Montreal Steak Spice,2072371
            Brandy - Bar,9763562
            Sprouts Dikon,4959776
            Wine - Chenin Blanc K.w.v.,8191183
            Smirnoff Green Apple Twist,4552849
            Beef - Prime Rib Aaa,6601628
            Doilies - 10, Paper,456531
            Tart Shells - Savory, 4,400501
            Cake - Cake Sheet Macaroon,1399206
            Beef - Diced,8329845
            Hot Chocolate - Individual,7813751
            Pork - Ham Hocks - Smoked,1016969
            Cup - 8oz Coffee Perforated,4177697
            Beans - Navy, Dry,1594233
            Pesto - Primerba, Paste,7477453
            Cattail Hearts,7337830
            Bandage - Finger Cots,5314864
            Mustard - Dijon,9878842
            Sprouts - Bean,4066319
            Beef - Ox Tongue, Pickled,4206746
            Mushroom - Portebello,7523571
            Tortillas - Flour, 12,4272838
            Sauce - Hollandaise,5582526
            Container - Clear 32 Oz,870556
            Pail - 4l White, With Handle,9865906
            Veal Inside - Provimi,8481221
            Wine - Kwv Chenin Blanc South,8602501
            Club Soda - Schweppes, 355 Ml,5943075
            Nantucket Cranberry Juice,802191
            Tea - Herbal I Love Lemon,6272988
            Calaloo,4769972
            Dried Figs,1209086
            Russian Prince,8891081
            Bay Leaf Ground,4922512
            Pear - Asian,2473636
            Chicken Thigh - Bone Out,7044427
            Egg - Salad Premix,4819066
            Pear - Halves,958763
            Liners - Banana, Paper,8764738
            Canada Dry,1679877
            Tray - 12in Rnd Blk,5322481
            The Pop Shoppe - Lime Rickey,2260028
            Sprouts - China Rose,1524738
            Beans - Yellow,4344798
            Wine - Zinfandel California 2002,1755363
            Cheese - Mascarpone,7836284
            V8 - Berry Blend,9891560
            Wine - Magnotta - Bel Paese White,5686238
            Ice Cream Bar - Hageen Daz To,8064079
            Noodles - Steamed Chow Mein,9564174
            Coffee - Decaffeinato Coffee,340191
            Cognac - Courvaisier,5420978
            Lobster - Canned Premium,4784811
            Cocoa Feuilletine,7762379
            Appetizer - Sausage Rolls,2470609
            Pork Loin Cutlets,5557171
            Tuna - Bluefin,7672684
            Oneshot Automatic Soap System,7795517
            Jam - Strawberry, 20 Ml Jar,8064035
            Kellogs Cereal In A Cup,2215966
            Tuna - Fresh,6436965
            Table Cloth 54x72 White,6446417
            Sole - Dover, Whole, Fresh,8370374
            Doilies - 12, Paper,296264
            Foil - 4oz Custard Cup,6807426
            Rice - Aborio,9387095
            Bread - Olive Dinner Roll,5959351
            Container - Hngd Cll Blk 7x7x3,775421
            Oil - Food, Lacquer Spray,2695423
            Mushroom - Porcini, Dry,7115663
            Cheese - Provolone,6817429
            V8 - Tropical Blend,2034529
            Pork - Bacon,back Peameal,3907335
            Lid Coffeecup 12oz D9542b,9990685
            Yeast Dry - Fleischman,5697105
            Soup - Knorr, Chicken Gumbo,6006277
            Mushroom - Crimini,5480304
            Fenngreek Seed,6682593
            Brocolinni - Gaylan, Chinese,7736858
            Danishes - Mini Raspberry,8946780
            Pail - 15l White, With Handle,8347128
            Pie Shell - 5,543537
            Plate Foam Laminated 9in Blk,361535
            Cheese - Sheep Milk,1897639
            Jam - Blackberry, 20 Ml Jar,232731
            Chocolate Liqueur - Godet White,9473920
            Bread - Raisin Walnut Oval,8927184
            Tea - Jasmin Green,8455215
            Wine - Saint - Bris 2002, Sauv,3275156
            Cheese - Bocconcini,6550572
            Oil - Peanut,7957824
            Rice Wine - Aji Mirin,7709648
            Petite Baguette,2504364
            Lobster - Live,1321465
            Napkin - Beverage 1 Ply,9343431
            Pepper - White, Whole,9035375
            Shrimp, Dried, Small / Lb,6603167
            Bouq All Italian - Primerba,2896595
            Sobe - Lizard Fuel,9641995
            Apple - Northern Spy,7479592
            Cleaner - Lime Away,5592492
            Ecolab - Orange Frc, Cleaner,3996660
            Soup Knorr Chili With Beans,1684240
            Oil - Peanut,9316888
            Pork - Bacon, Sliced,5471409
            Mix - Cocktail Strawberry Daiquiri,6009529
            Doilies - 5, Paper,6787381
            Potato - Sweet,3936676
            Lentils - Green Le Puy,5150204
            Island Oasis - Wildberry,481733
            Beef - Salted,9005785
            Wine - Vovray Sec Domaine Huet,4751222
            Pork - Back, Short Cut, Boneless,7624406
            Dried Apple,4142846
            Bread - Calabrese Baguette,7905296
            Pasta - Fusili Tri - Coloured,783833
            Soup Campbells Mexicali Tortilla,7973919
            Bread Base - Toscano,8182724
            Flour - Corn, Fine,7165604
            Juice - Apple, 1.36l,4421029
            Cod - Fillets,3858962
            Poppy Seed,9801653
            Jolt Cola - Red Eye,4933762
            Beer - Camerons Auburn,2245536
            Wine - Red Oakridge Merlot,6637412
            Bread - White Epi Baguette,2448777
            Olives - Kalamata,6606948
            Cookies Oatmeal Raisin,2136790
            Water - Perrier,4296081
            Pickle - Dill,4883897
            Cheese - Gorgonzola,4982821
            Sugar - Sweet N Low, Individual,8228265
            Mushroom - Chanterelle, Dry,7889173
            Buffalo - Short Rib Fresh,8024387
            Creme De Banane - Marie,1626698
            Crab - Meat Combo,917860
            Cake - Lemon Chiffon,7544478
            Silicone Parch. 16.3x24.3,1595024
            Yogurt - French Vanilla,2593761
            Compound - Rum,2190354
            Ecolab - Hand Soap Form Antibac,338634
            Daikon Radish,7577136
            Maple Syrup,2065293
            Ketchup - Tomato,6589812
            Pastry - Apple Muffins - Mini,3999685
            Cream - 10%,8805115
            Flour - All Purpose,9883000
            Spice - Pepper Portions,7217889
            White Fish - Filets,9953828
            Turnip - Mini,9209268
            Ginger - Ground,8345179
            Potatoes - Purple, Organic,5140254
            Extract - Raspberry,8461304
            Lobster - Cooked,7005944
            Soupfoamcont12oz 112con,4141758
            Juice - Grapefruit, 341 Ml,7169772
            Pastry - Key Limepoppy Seed Tea,4393905
            Sponge Cake Mix - Chocolate,2808766
            Chicken - Livers,4326167
            Sandwich Wrap,1588701
            Rum - Spiced, Captain Morgan,5913788
            Filter - Coffee,378477
            Wine - Sawmill Creek Autumn,8345418
            Nut - Hazelnut, Whole,9322977
            Puree - Passion Fruit,7613190
            Foil - Round Foil,6916795
            Pork - Bacon Cooked Slcd,1048163
            Nut - Almond, Blanched, Whole,1004420
            Mushroom - Lg - Cello,6834274
            Milk Powder,9418588
            Sesame Seed Black,9957494
            Sugar - Cubes,5656522
            Tea - Earl Grey,4394952
            Beer - Blue Light,6381172
            Raisin - Dark,8543391
            Bread - Italian Corn Meal Poly,3149393
            Frangelico,6202051
            Tart Shells - Sweet, 3,1249956
            Whmis Spray Bottle Graduated,7656417
            Shrimp - Prawn,6550998
            Ham - Cooked,6397416
            Squid - Breaded,7168869
            Wine - Red, Metus Rose,2261194
            Table Cloth 53x53 White,3573392
            Seaweed Green Sheets,9820553
            Cake - Box Window 10x10x2.5,4243901
            Tabasco Sauce, 2 Oz,120460
            Crab - Imitation Flakes,4006206
            Stock - Veal, White,8056941
            Trout - Rainbow, Frozen,8554269
            Muffin Mix - Oatmeal,7797160
            Chicken - Ground,2218815
            Aspic - Amber,3737530
            Shrimp - 150 - 250,778461
            Oxtail - Cut,9796023
            Chevere Logs,7083549
            Veal - Insides, Grains,6005412
            Pail For Lid 1537,4427773
            Eggplant - Baby,5365295
            Sour Puss Raspberry,5953798
            Soup - Campbells, Creamy,7803509
            Cinnamon - Stick,6100060
            Glucose,2521995
            Fish - Soup Base, Bouillon,182905
            Duck - Breast,4030277
            Trout - Smoked,9569787
            Langers - Ruby Red Grapfruit,3956106
            Watercress,6252873
            Appetizer - Chicken Satay,5272220
            Sambuca - Opal Nera,291276
            Emulsifier,2486902
            Muffin Hinge 117n,8105018
            Oil - Margarine,6179031
            Soupcontfoam16oz 116con,2133002
            Jicama,3231002
            Vacuum Bags 12x16,8819486
            Salami - Genova,7909953
            Onions - Cippolini,943817
            Sauce - Chili,8680502
            Flavouring Vanilla Artificial,7953560
            Pickles - Gherkins,8986308
            Quail Eggs - Canned,9735194
            Oil - Peanut,8180848
            Veal - Osso Bucco,8099954
            Schnappes - Peach, Walkers,4385549
            Table Cloth 120 Round White,2219913
            Tea - Apple Green Tea,1963529
            Monkfish Fresh - Skin Off,6009227
            Beans - Kidney White,7472596
            Bonito Flakes - Toku Katsuo,9603072
            Cheese - Perron Cheddar,4136314
            Beer - Molson Excel,1040167
            Longan,4512060
            Jolt Cola,2131233
            Crush - Grape, 355 Ml,5376623
            Veal - Striploin,3186088
            Chicken - Wings, Tip Off,4478153
            Goat - Whole Cut,8611228
            Milk - Chocolate 500ml,1172932
            Fish - Atlantic Salmon, Cold,134305
            Cleaner - Comet,3627242
            Tandoori Curry Paste,8243167
            Muffin Mix - Lemon Cranberry,2309001
            Dasheen,9825358
            Shrimp - Black Tiger 26/30,5867008
            Wine - Maipo Valle Cabernet,5755158
            C - Plus, Orange,1029154
            Okra,2011608
            Extract - Raspberry,483077
            Sprouts - Onion,3218169
            Wine - Segura Viudas Aria Brut,9354903
            Beef - Rib Roast, Cap On,5933536
            Herb Du Provence - Primerba,6799794
            Quiche Assorted,2342293
            Lettuce - Spring Mix,4516553
            Cheese - Montery Jack,7887286
            Bread - Bistro Sour,4804479
            Milk - Condensed,3457784
            Trout - Rainbow, Frozen,6243866
            Wine - Barossa Valley Estate,5530638
            Wine - Prem Select Charddonany,343815
            Wine - Zinfandel California 2002,3270278
            Snapple - Mango Maddness,8498192
            Soup - Knorr, Veg / Beef,9929888
            Ecolab - Solid Fusion,1335229
            Gingerale - Diet - Schweppes,3054265
            Crackers - Trio,6209339
            Instant Coffee,3129762
            Trout - Rainbow, Fresh,6373379
            Soup - Campbells Beef Stew,6127780
            Juice - Cranberry, 341 Ml,4360891
            Bread Country Roll,4937560
            Veal - Brisket, Provimi,bnls,2619304
            Mace Ground,159181
            Paper - Brown Paper Mini Cups,4120693
            Sauce - White, Mix,6871946
            Lettuce Romaine Chopped,9142921
            Food Colouring - Green,8468741
            Versatainer Nc - 8288,8581352
            Beans - Yellow,8894256
            Shrimp - 150 - 250,1091882
            Taro Root,336820
            Cheese - Parmigiano Reggiano,4675667
            Lamb - Pieces, Diced,6479803
            Flower - Daisies,5886842
            Wine - Sake,2251523
            Foil - Round Foil,7102904
            Energy Drink Bawls,119264
            Contreau,8133909
            Wine - Rioja Campo Viejo,8594825
            Cafe Royale,174361
            Sugar - Brown, Individual,5547074
            Apple - Royal Gala,3021172
            Clementine,5493605
            Pepper - Green Thai,3176365
            Island Oasis - Ice Cream Mix,9093739
            Rabbit - Saddles,7073940
            Pail - 15l White, With Handle,4118596
            Nantucket - Carrot Orange,1905955
            Pastry - French Mini Assorted,9672385
            Raspberries - Fresh,2804554
            The Pop Shoppe - Grape,9309980
            Oats Large Flake,9031419
            Mop Head - Cotton, 24 Oz,4022768
            Pastry - Choclate Baked,6719684
            Veal - Knuckle,3049350
            Chambord Royal,8336212
            Pork - Liver,3342566
            Chef Hat 20cm,4732264
            Rum - Cream, Amarula,4546210
            Veal - Nuckle,5961318
            Lamb - Bones,2038989
            Cheese - Mascarpone,3706296
            Soup - Campbells, Creamy,4863836
            Black Currants,3400112
            Shrimp - 16/20, Iqf, Shell On,1912141
            Squash - Pattypan, Yellow,1971387
            Table Cloth 62x120 Colour,6596312
            Cheese - Stilton,8002413
            Muffin Mix - Carrot,1078321
            Relish,7305206
            Soup - Campbellschix Stew,4384307
            Wine - Ej Gallo Sonoma,7721759
            Pastry - Carrot Muffin - Mini,6641706
            Pork - Smoked Back Bacon,237967
            Cheese - Camembert,5560817
            Creamers - 10%,6611691
            Wine - Remy Pannier Rose,3915905
            Paste - Black Olive,9372727
            Wine - Chablis J Moreau Et Fils,4879328
            Beans - Yellow,5864432
            Myers Planters Punch,9736249
            Beef - Inside Round,4437658
            Sugar - Cubes,2890466
            Thyme - Fresh,2868080
            Chocolate - Milk, Callets,461013
            Foam Tray S2,6027984
            Cookie Dough - Peanut Butter,9124709
            Milk 2% 500 Ml,7734365
            Lentils - Red, Dry,269193
            Browning Caramel Glace,5538712
            Sauce - Demi Glace,4451539
            Yogurt - Strawberry, 175 Gr,7850222
            Doilies - 12, Paper,367105
            Doilies - 10, Paper,9637212
            Wheat - Soft Kernal Of Wheat,2229998
            Fish - Base, Bouillion,1383404
            Rabbit - Whole,9801079
            Chicken - Leg / Back Attach,2370541
            Cake - Lemon Chiffon,1627686
            Syrup - Monin, Swiss Choclate,5502904
            Pork - Backfat,7093128
            Melon - Honey Dew,795865
            Sauce - Balsamic Viniagrette,6650546
            Sauce - Balsamic Viniagrette,9531601
            Pork - Sausage Casing,1473102
            Pasta - Rotini, Colour, Dry,121538
            Chicken - White Meat With Tender,2682443
            Soup - Beef, Base Mix,9349057
            Nantucket Orange Juice,9687275
            Otomegusa Dashi Konbu,8797347
            Kohlrabi,4113882
            Olives - Stuffed,9524620
            Cheese - Cheddar, Medium,294772
            Pasta - Detalini, White, Fresh,3506898
            Tea - Black Currant,3390189
            Goulash Seasoning,2205517
            Cocktail Napkin Blue,309424
            Bagel - Ched Chs Presliced,4925043
            Cheese - Manchego, Spanish,6038038
            Frangelico,2455368
            Melon - Honey Dew,8006866
            Versatainer Nc - 9388,8393165
            Soho Lychee Liqueur,346749
            Cheese - Woolwich Goat, Log,7939650
            Veal - Ground,2651308
            Flour - Buckwheat, Dark,4956279
            Containter - 3oz Microwave Rect.,2668424
            Nantucket - Orange Mango Cktl,1841181
            Corn - On The Cob,2610795
            Cauliflower,2728086
            Shrimp - 16/20, Peeled Deviened,1467437
            Butter Sweet,204277
            Soup - Campbells Broccoli,3554133
            Jam - Blackberry, 20 Ml Jar,2852843
            Juice - Pineapple, 48 Oz,9941965
            Apple - Royal Gala,1327221
            Orange Roughy 6/8 Oz,7996840
            Cheese - Grie Des Champ,2560880
            Veal - Slab Bacon,6041414
            Tart Shells - Sweet, 3,2634615
            Jerusalem Artichoke,1379879
            Lentils - Green Le Puy,3272038
            Worcestershire Sauce,1164827
            Danishes - Mini Raspberry,3085640
            Flour - Bran, Red,2614731
            Soup - Campbells Tomato Ravioli,722900
            Sole - Dover, Whole, Fresh,7317978
            Wine - Spumante Bambino White,4754601
            Coconut - Creamed, Pure,6694172
            Olives - Black, Pitted,3527640
            Island Oasis - Magarita Mix,3454550
            Beef - Roasted, Cooked,1819262
            Nectarines,1172840
            Nut - Walnut, Pieces,1624602
            Ham - Proscuitto,8462595
            Orange - Canned, Mandarin,6906785
            Beef - Short Loin,8458172
            Halibut - Fletches,1579756
            Oil - Olive,7258936
            Pasta - Elbows, Macaroni, Dry,9327731
            Cheese - Swiss,1596628
            Muffin - Mix - Creme Brule 15l,5923602
            Monkfish - Fresh,5943928
            Bananas,9707573
            Fennel,3260619
            Silicone Parch. 16.3x24.3,686351
            Coke - Classic, 355 Ml,2469427
            Bread - White Mini Epi,721959
            Soup Campbells Split Pea And Ham,8847066
            Wine - George Duboeuf Rose,9066337
            Bread - Rolls, Corn,1292091
            Cucumber - Pickling Ontario,2099530
            Wine - Toasted Head,2244082
            Oil - Cooking Spray,134491
            Bacardi Mojito,2031563
            Cheese - Cambozola,6732838
            Gelatine Leaves - Bulk,2089556
            Lotus Leaves,1357276
            Pork - Hock And Feet Attached,6052961
            Wine - Magnotta - Red, Baco,6673376
            Basil - Thai,768039
            Bread Foccacia Whole,9851289
            Flour - So Mix Cake White,5809666
            Tea - Earl Grey,4103106
            Beans - Kidney White,1436338
            Arctic Char - Fillets,5683391
            Mustard Prepared,1377829
            Lettuce Romaine Chopped,3564218
            Tequila Rose Cream Liquor,4448992
            Potatoes - Parissienne,9291439
            Veal - Liver,9887959
            Plastic Arrow Stir Stick,1679928
            Flour - Buckwheat, Dark,4453700
            Gelatine Leaves - Envelopes,205755
            Wine - White, French Cross,1045940
            Beer - True North Lager,1759925
            Pork - Bacon, Double Smoked,5049523
            Red Pepper Paste,8772245
            Soy Protein,571914
            Butter - Unsalted,9475784
            Yeast - Fresh, Fleischman,4275401
            Langers - Cranberry Cocktail,850149
            Muffin - Carrot Individual Wrap,6434102
            Fudge - Chocolate Fudge,1525198
            Potatoes - Idaho 100 Count,8708615
            Apple - Delicious, Golden,7771226
            Apples - Spartan,5273478
            Chips Potato Reg 43g,4651914
            Tart Shells - Savory, 2,6653993
            Bacardi Breezer - Tropical,5306446
            Corn - On The Cob,2958084
            Miso Paste White,6059405
            Rice Paper,3379735
            Kirsch - Schloss,4649509
            Chicken Thigh - Bone Out,5845348
            Knife Plastic - White,187364
            Wine - Saint Emilion Calvet,9907125
            Flour - Bread,8188808
            Pork - Bacon Cooked Slcd,974369
            Steampan - Half Size Shallow,7535143
            Wine - White Cab Sauv.on,6309837
            Mahi Mahi,3534572
            Butcher Twine 4r,6423821
            Carrots - Mini, Stem On,3886901
            Lighter - Bbq,5262340
            Puree - Strawberry,1575581
            Turnip - Wax,4826920
            ";

        #endregion Product Data

        foreach (var line in productData.Split('\r'))
        {
            var lineTrimmed = line.Trim();
            if (string.IsNullOrEmpty(lineTrimmed))
            {
                continue;
            }

            var lastCommaIndex = lineTrimmed.LastIndexOf(',');
            if (lastCommaIndex < 0)
            {
                continue;
            }

            var priceText = lineTrimmed[(lastCommaIndex + 1)..];
            if (!decimal.TryParse(priceText.Trim(), out decimal price))
            {
                continue;
            }

            var product = new Product
            {
                Name = lineTrimmed[..lastCommaIndex],
                Price = price,
            };

            await productSet.AddAsync(product);
        }

        await context.SaveChangesAsync();
    }

    private async Task AddOrders(DbContext context)
    {
        var orderSet = context.Set<Order>();

        var randomOrder = await orderSet.FirstOrDefaultAsync();
        if (randomOrder is not null)
        {
            return;
        }

        var customerSet = context.Set<Customer>();
        var productSet = context.Set<Product>();

        var customerIds = await customerSet.Select(c => c.Id).ToListAsync();
        var products = await productSet.ToListAsync();
        var productCount = products.Count;

        foreach (var customerId in customerIds)
        {
            var numberOfOrders = RandomNumberGenerator.GetInt32(1, 10);

            for (var orderIndex = 1; orderIndex < numberOfOrders + 1; orderIndex++)
            {
                var numberOfProducts = RandomNumberGenerator.GetInt32(1, 10);
                var orderProducts = new List<OrderProduct>();

                decimal total = 0;

                for (var productIndex = 0; productIndex < numberOfProducts + 1; productIndex++)
                {
                    var product = products[productIndex];
                    do
                    {
                        var currentProductIndex = RandomNumberGenerator.GetInt32(0, productCount - 1);
                        product = products[currentProductIndex];
                    } while (orderProducts.Exists(p => product.Id == p.ProductId));

                    var quantity = RandomNumberGenerator.GetInt32(1, 100);
                    orderProducts.Add(new OrderProduct
                    {
                        ProductId = product.Id,
                        Quantity = quantity
                    });

                    total += (product.Price ?? 0) * quantity;
                }

                var order = new Order
                {
                    CustomerId = customerId,
                    OrderDate = DateTime.UtcNow,
                    Products = orderProducts,
                    Total = total,
                    DiscountAmount = 0
                };

                await orderSet.AddAsync(order);
            }
        }

        await context.SaveChangesAsync();
    }
}
