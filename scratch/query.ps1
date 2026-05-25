$connectionString = "Data Source=.;Initial Catalog=Saowari;Integrated Security=True;MultipleActiveResultSets=True;TrustServerCertificate=True"
$connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$connection.Open()

# Query 1: Join BookingSeats, Booking, Seat
$query = "SELECT BS.BookingSeatId, BS.BookingId, BS.SeatId, B.ScheduleID, S.SeatNumber FROM BookingSeats BS JOIN Booking B ON BS.BookingId = B.BookingID JOIN Seat S ON BS.SeatId = S.SeatID WHERE B.ScheduleID = 12"
$command = New-Object System.Data.SqlClient.SqlCommand($query, $connection)
$reader = $command.ExecuteReader()
Write-Host "--- BookingSeats records for Schedule 12 ---"
while ($reader.Read()) {
    Write-Host ("BookingSeatID: " + $reader["BookingSeatId"] + " | BookingID: " + $reader["BookingId"] + " | SeatID: " + $reader["SeatId"] + " | SeatNumber: " + $reader["SeatNumber"])
}
$reader.Close()

$connection.Close()
